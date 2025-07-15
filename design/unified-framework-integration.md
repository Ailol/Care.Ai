# Unified Framework Integration Design

## Common Patterns Across AI Frameworks

### 1. **Capability Discovery**
- **Semantic Kernel**: Plugins with Functions, each having metadata (name, description, parameters)
- **LangChain**: Tools with name, description, and args schema
- **LlamaIndex**: Query engines and tools with metadata and descriptions
- **Common Pattern**: All expose operations as named functions with descriptions and parameter schemas

### 2. **Execution Pattern**
- **Semantic Kernel**: `InvokeAsync(function, arguments)`
- **LangChain**: `tool.invoke(args)` or `chain.invoke(input)`
- **LlamaIndex**: `agent.run(query)` or `tool(args)`
- **Common Pattern**: Single execution method with arguments as dictionary/object

### 3. **Registration Pattern**
- **Semantic Kernel**: `kernel.ImportPlugin()` or `kernel.AddFunction()`
- **LangChain**: `tools = [tool1, tool2]` then bind to LLM
- **LlamaIndex**: `agent = Agent.from_tools(tools)`
- **Common Pattern**: Framework accepts a collection of capabilities

## Proposed Unified Framework Interface

```csharp
// Base capability interface that all frameworks must implement
public interface IFrameworkCapability
{
    string Name { get; }
    string Description { get; }
    JsonDocument? ParameterSchema { get; }
    Dictionary<string, object>? Metadata { get; }
}

// Framework adapter interface
public interface IFrameworkAdapter
{
    string FrameworkType { get; } // "semantic-kernel", "langchain", "llamaindex"
    string FrameworkVersion { get; }
    
    // Capability discovery
    Task<IReadOnlyList<IFrameworkCapability>> DiscoverCapabilitiesAsync(
        CancellationToken cancellationToken = default);
    
    // Execution
    Task<IResponse> ExecuteCapabilityAsync(
        string capabilityName,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken = default);
    
    // Lifecycle
    Task InitializeAsync(Dictionary<string, object> configuration);
    Task ShutdownAsync();
}

// Registration in KernelBuilder
public class KernelBuilder
{
    // For in-process frameworks (NuGet)
    public KernelBuilder AddFramework<TAdapter>(
        string instanceId, 
        Action<FrameworkConfig> configure) 
        where TAdapter : IFrameworkAdapter
    {
        var config = new FrameworkConfig();
        configure(config);
        
        // Register as singleton with factory
        _services.AddSingleton<IFrameworkAdapter>(sp =>
        {
            var adapter = ActivatorUtilities.CreateInstance<TAdapter>(sp);
            adapter.InitializeAsync(config.Options).GetAwaiter().GetResult();
            return adapter;
        });
        
        // Track registration
        _frameworks.Add(new FrameworkRegistration
        {
            InstanceId = instanceId,
            AdapterType = typeof(TAdapter),
            ExecutionMode = "in-process",
            Configuration = config
        });
        
        return this;
    }
    
    // For remote frameworks (HTTP/gRPC)
    public KernelBuilder AddRemoteFramework(
        string instanceId,
        Action<RemoteFrameworkConfig> configure)
    {
        var config = new RemoteFrameworkConfig();
        configure(config);
        
        _frameworks.Add(new FrameworkRegistration
        {
            InstanceId = instanceId,
            ExecutionMode = "remote",
            Configuration = config
        });
        
        return this;
    }
    
    // For MCP servers
    public KernelBuilder AddMcpFramework(
        string instanceId,
        Action<McpFrameworkConfig> configure)
    {
        var config = new McpFrameworkConfig();
        configure(config);
        
        _frameworks.Add(new FrameworkRegistration
        {
            InstanceId = instanceId,
            ExecutionMode = "mcp",
            Configuration = config
        });
        
        return this;
    }
}
```

## Adapter Implementations

### Semantic Kernel Adapter
```csharp
public class SemanticKernelAdapter : IFrameworkAdapter
{
    private Microsoft.SemanticKernel.Kernel _kernel;
    
    public async Task<IReadOnlyList<IFrameworkCapability>> DiscoverCapabilitiesAsync(
        CancellationToken cancellationToken)
    {
        var capabilities = new List<IFrameworkCapability>();
        
        foreach (var plugin in _kernel.Plugins)
        {
            foreach (var function in plugin)
            {
                capabilities.Add(new FrameworkCapability
                {
                    Name = $"{plugin.Name}.{function.Name}",
                    Description = function.Description,
                    ParameterSchema = ConvertToJsonSchema(function.Metadata.Parameters)
                });
            }
        }
        
        return capabilities;
    }
    
    public async Task<IResponse> ExecuteCapabilityAsync(
        string capabilityName,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var parts = capabilityName.Split('.');
        var result = await _kernel.InvokeAsync(
            pluginName: parts[0],
            functionName: parts[1],
            arguments: new KernelArguments(parameters),
            cancellationToken);
            
        return new Response
        {
            Success = true,
            Result = result.GetValue<object>(),
            Type = "semantic-kernel-result"
        };
    }
}
```

### LangChain Adapter
```csharp
public class LangChainAdapter : IFrameworkAdapter
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    
    public async Task<IReadOnlyList<IFrameworkCapability>> DiscoverCapabilitiesAsync(
        CancellationToken cancellationToken)
    {
        // LangChain exposes tools via API
        var response = await _httpClient.GetAsync($"{_baseUrl}/tools", cancellationToken);
        var tools = await response.Content.ReadFromJsonAsync<List<LangChainTool>>();
        
        return tools.Select(t => new FrameworkCapability
        {
            Name = t.Name,
            Description = t.Description,
            ParameterSchema = JsonDocument.Parse(t.ArgsSchema)
        }).ToList();
    }
    
    public async Task<IResponse> ExecuteCapabilityAsync(
        string capabilityName,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"{_baseUrl}/invoke",
            new { tool = capabilityName, args = parameters },
            cancellationToken);
            
        var result = await response.Content.ReadFromJsonAsync<LangChainResult>();
        
        return new Response
        {
            Success = response.IsSuccessStatusCode,
            Result = result?.Output,
            Type = "langchain-result"
        };
    }
}
```

### LlamaIndex Adapter
```csharp
public class LlamaIndexAdapter : IFrameworkAdapter
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    
    public async Task<IReadOnlyList<IFrameworkCapability>> DiscoverCapabilitiesAsync(
        CancellationToken cancellationToken)
    {
        // LlamaIndex exposes query engines and tools
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/tools", cancellationToken);
        var tools = await response.Content.ReadFromJsonAsync<List<LlamaIndexTool>>();
        
        return tools.Select(t => new FrameworkCapability
        {
            Name = t.Name,
            Description = t.Description,
            ParameterSchema = JsonDocument.Parse(t.Metadata.ParameterSchema),
            Metadata = new Dictionary<string, object>
            {
                ["tool_type"] = t.ToolType, // "query_engine", "function", etc.
                ["return_direct"] = t.ReturnDirect
            }
        }).ToList();
    }
    
    public async Task<IResponse> ExecuteCapabilityAsync(
        string capabilityName,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"{_baseUrl}/api/execute",
            new { tool = capabilityName, args = parameters },
            cancellationToken);
            
        var result = await response.Content.ReadFromJsonAsync<LlamaIndexResult>();
        
        return new Response
        {
            Success = response.IsSuccessStatusCode,
            Result = result?.Output,
            Type = "llamaindex-result",
            Metadata = result?.Metadata
        };
    }
}
```

## Usage Example

```csharp
builder.Services.AddCareAiKernel(config, kernelBuilder =>
{
    // In-process Semantic Kernel instances
    kernelBuilder.AddFramework<SemanticKernelAdapter>("sk-gpt4", cfg =>
    {
        cfg.Options["Model"] = "gpt-4";
        cfg.Options["Endpoint"] = "https://api.openai.com/v1";
    });
    
    kernelBuilder.AddFramework<SemanticKernelAdapter>("sk-local", cfg =>
    {
        cfg.Options["Model"] = "llama2";
        cfg.Options["Endpoint"] = "http://localhost:11434/v1";
    });
    
    // Remote LangChain server
    kernelBuilder.AddRemoteFramework("langchain-prod", cfg =>
    {
        cfg.BaseUrl = "https://langchain-api.company.com";
        cfg.FrameworkType = "langchain";
        cfg.AuthType = "bearer";
        cfg.AuthToken = config["LangChain:Token"];
    });
    
    // LlamaIndex via MCP
    kernelBuilder.AddMcpFramework("llamaindex-mcp", cfg =>
    {
        cfg.Transport = "stdio";
        cfg.Command = "python";
        cfg.Args = "-m llamaindex_mcp_server";
        cfg.FrameworkType = "llamaindex";
    });
    
    // Another team's Semantic Kernel API
    kernelBuilder.AddRemoteFramework("team-b-sk", cfg =>
    {
        cfg.BaseUrl = "http://team-b-api:5000";
        cfg.FrameworkType = "semantic-kernel";
        cfg.DiscoveryEndpoint = "/api/plugins";
        cfg.ExecuteEndpoint = "/api/execute";
    });
});
```

## MCP Integration

For MCP servers, the adapter would handle the MCP protocol:

```csharp
public class McpFrameworkAdapter : IFrameworkAdapter
{
    private readonly IMcpClient _mcpClient;
    private readonly string _frameworkType;
    
    public async Task<IReadOnlyList<IFrameworkCapability>> DiscoverCapabilitiesAsync(
        CancellationToken cancellationToken)
    {
        // MCP tools/resources discovery
        var tools = await _mcpClient.ListToolsAsync(cancellationToken);
        
        return tools.Select(t => new FrameworkCapability
        {
            Name = t.Name,
            Description = t.Description,
            ParameterSchema = t.InputSchema,
            Metadata = new Dictionary<string, object>
            {
                ["mcp_tool"] = true,
                ["framework_type"] = _frameworkType
            }
        }).ToList();
    }
    
    public async Task<IResponse> ExecuteCapabilityAsync(
        string capabilityName,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var result = await _mcpClient.CallToolAsync(
            capabilityName, 
            parameters, 
            cancellationToken);
            
        return new Response
        {
            Success = result.IsSuccess,
            Result = result.Content,
            Type = $"mcp-{_frameworkType}-result"
        };
    }
}
```

## Key Design Principles

1. **No Framework-Specific Types**: The kernel only knows about capabilities (name, description, parameters)
2. **Unified Execution**: All frameworks execute through `ExecuteCapabilityAsync(name, params)`
3. **Dynamic Discovery**: Capabilities are discovered at runtime, not hardcoded
4. **Transport Agnostic**: Same interface works for in-process, HTTP, gRPC, or MCP
5. **Framework Agnostic**: The kernel doesn't care if it's SK, LC, or LI - just capabilities
6. **Instance-Based**: Multiple instances of the same framework with different configurations

## Benefits

- **Zero Framework Lock-in**: Switch between SK, LC, LI without code changes
- **Multi-Instance Support**: Run multiple GPT-4, Llama, etc. instances simultaneously
- **Deployment Flexibility**: Same code works as NuGet, remote API, or MCP server
- **Auto-Discovery**: Capabilities are discovered, not hardcoded
- **Framework Evolution**: New frameworks can be added without kernel changes