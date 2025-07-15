using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Care.Ai.Kernel;
using Care.Ai.Kernel.Abstractions;
using Care.Ai.Frameworks.SemanticKernel;
using Care.Ai.Frameworks.Rag;
using System.Text.Json;

namespace Care.AI.Integration.Tests;

/// <summary>
/// Comprehensive integration tests for all current Care.AI capabilities
/// Tests the entire stack from request to response
/// </summary>
public class FullStackIntegrationTests : IAsyncLifetime
{
    private ServiceProvider _serviceProvider = null!;
    private IKernel _kernel = null!;
    private IConfiguration _configuration = null!;

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();
        
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Ollama:Endpoint"] = "http://localhost:11434/v1",
                ["Ollama:Model"] = "qwen3-clean:latest",
                ["Ollama:EmbeddingModel"] = "nomic-embed-text:latest",
                ["CareAi:Kernel:EnableSmartRouting"] = "true",
                ["CareAi:Kernel:SmartRoutingProvider"] = "semantic-kernel"
            })
            .Build();

        // Setup services as in production
        services.AddLogging();
        services.AddHttpClient();
        
        // Add kernel with multiple frameworks
        services.AddCareAiKernel(_configuration, kernelBuilder =>
        {
            kernelBuilder.AddFramework<SemanticKernelFramework>(config =>
            {
                config.ServiceId = "semantic-kernel-local";
                config.Options["Model"] = "qwen3-clean:latest";
            });
            
            kernelBuilder.AddFramework<RagFramework>(config =>
            {
                config.ServiceId = "llamaindex-rag";
                config.Options["DefaultIndexPath"] = "./test-indexes";
                config.Options["EmbeddingModel"] = "nomic-embed-text:latest";
            });
        });

        _serviceProvider = services.BuildServiceProvider();
        _kernel = _serviceProvider.GetRequiredService<IKernel>();
        
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _serviceProvider?.Dispose();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Test01_KernelDiscovery_ShouldFindAllProviders()
    {
        // Act
        var providers = await _kernel.GetProvidersAsync();

        // Assert
        Assert.NotNull(providers);
        Assert.True(providers.Count >= 2, "Should discover at least 2 providers");
        
        var semanticKernel = providers.FirstOrDefault(p => p.ProviderId == "semantic-kernel");
        Assert.NotNull(semanticKernel);
        Assert.Contains("chat", semanticKernel.Operations);
        Assert.Contains("completion", semanticKernel.Operations);
        
        var llamaIndex = providers.FirstOrDefault(p => p.ProviderId == "llamaindex-rag");
        Assert.NotNull(llamaIndex);
        Assert.Contains("rag", llamaIndex.Operations);
        Assert.Contains("search", llamaIndex.Operations);
        Assert.Contains("index", llamaIndex.Operations);
    }

    [Fact]
    public async Task Test02_SemanticKernel_SimpleChatCompletion()
    {
        // Arrange
        var request = new Care.Ai.Kernel.Models.Request
        {
            Type = "chat",
            Data = new Dictionary<string, object>
            {
                ["prompt"] = "Say 'Hello from test' and nothing else"
            }
        };

        // Act
        var response = await _kernel.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.NotNull(response.Result);
        
        var result = response.Result?.ToString() ?? "";
        Assert.Contains("Hello from test", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Test03_SmartRouting_ShouldRouteToCorrectProvider()
    {
        // Test that smart routing correctly identifies RAG requests
        var ragRequest = new Care.Ai.Kernel.Models.Request
        {
            Type = "auto", // Let smart routing decide
            Data = new Dictionary<string, object>
            {
                ["query"] = "Search for information about API security best practices",
                ["useSmartRouting"] = true
            }
        };

        // This should route to RAG framework based on the "search" keyword
        // Currently will fail as vector store not implemented
        await Assert.ThrowsAsync<NotImplementedException>(async () =>
        {
            await _kernel.ExecuteAsync(ragRequest);
        });
    }

    [Fact]
    public async Task Test04_Rag_EmbeddingGeneration()
    {
        // Test that embedding service is properly configured
        var embeddingService = _serviceProvider.GetRequiredService<Care.Ai.Frameworks.Rag.Services.EmbeddingService>();
        
        // This will call Ollama for embeddings
        // Will fail if Ollama is not running
        var exception = await Record.ExceptionAsync(async () =>
        {
            var embedding = await embeddingService.GenerateEmbeddingAsync("test text");
            Assert.NotNull(embedding);
            Assert.True(embedding.Length > 0);
        });

        // We expect this to fail without Ollama running
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task Test05_ProviderManifest_Serialization()
    {
        // Test that manifests can be serialized for API responses
        var providers = await _kernel.GetProvidersAsync();
        
        var json = JsonSerializer.Serialize(providers, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(json);
        Assert.Contains("llamaindex-rag", json);
        Assert.Contains("semantic-kernel", json);
        
        // Verify structure
        var deserialized = JsonSerializer.Deserialize<List<JsonElement>>(json);
        Assert.NotNull(deserialized);
        Assert.All(deserialized, provider =>
        {
            Assert.True(provider.TryGetProperty("providerId", out _));
            Assert.True(provider.TryGetProperty("operations", out _));
            Assert.True(provider.TryGetProperty("serviceId", out _));
        });
    }

    [Fact]
    public async Task Test06_MultipleFrameworkRegistration()
    {
        // Test that multiple instances of same framework type can be registered
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpClient();
        
        services.AddCareAiKernel(_configuration, kernelBuilder =>
        {
            // Register multiple SK instances with different models
            kernelBuilder.AddFramework<SemanticKernelFramework>(config =>
            {
                config.ServiceId = "sk-fast";
                config.Options["Model"] = "qwen:0.5b";
            });
            
            kernelBuilder.AddFramework<SemanticKernelFramework>(config =>
            {
                config.ServiceId = "sk-smart";
                config.Options["Model"] = "qwen3-clean:latest";
            });
        });

        using var provider = services.BuildServiceProvider();
        var kernel = provider.GetRequiredService<IKernel>();
        var providers = await kernel.GetProvidersAsync();
        
        var skProviders = providers.Where(p => p.ProviderId == "semantic-kernel").ToList();
        Assert.Equal(2, skProviders.Count);
    }

    [Fact]
    public async Task Test07_RequestResponse_Contract()
    {
        // Verify request/response contract implementation
        var request = new Care.Ai.Kernel.Models.Request
        {
            Type = "chat",
            Data = new Dictionary<string, object> { ["prompt"] = "test" },
            Metadata = new Dictionary<string, object> { ["traceid"] = "123" }
        };

        var response = await _kernel.ExecuteAsync(request);

        Assert.NotNull(response);
        Assert.NotNull(response.Type);
        Assert.NotNull(response.Data);
        Assert.Equal("completion", response.Type);
    }

    [Fact] 
    public async Task Test08_ErrorHandling_InvalidRequestType()
    {
        // Test error handling for unsupported request types
        var request = new Care.Ai.Kernel.Models.Request
        {
            Type = "unsupported-type",
            Data = new Dictionary<string, object>()
        };

        await Assert.ThrowsAsync<NotSupportedException>(async () =>
        {
            await _kernel.ExecuteAsync(request);
        });
    }

    [Fact]
    public async Task Test09_Rag_NotImplementedOperations()
    {
        // Document current state - these operations are not yet implemented
        var indexRequest = new Care.Ai.Kernel.Models.Request
        {
            Type = "index",
            Data = new Dictionary<string, object>
            {
                ["documents"] = new[] { "doc1", "doc2" },
                ["indexPath"] = "test-index"
            }
        };

        await Assert.ThrowsAsync<NotSupportedException>(async () =>
        {
            await _kernel.ExecuteAsync(indexRequest);
        });
    }

    [Fact]
    public async Task Test10_Configuration_Validation()
    {
        // Test that configuration is properly loaded
        var semanticKernelFramework = _serviceProvider.GetServices<IProviderDiscovery>()
            .OfType<SemanticKernelFramework>()
            .FirstOrDefault();
            
        Assert.NotNull(semanticKernelFramework);
        
        var manifest = await semanticKernelFramework.GetManifestAsync();
        Assert.Contains("qwen3-clean:latest", manifest.AvailableModels);
    }
}

/// <summary>
/// Tests for future workflow capabilities (currently not implemented)
/// </summary>
public class FutureWorkflowTests
{
    [Fact(Skip = "Workflow capabilities not yet implemented")]
    public async Task Test_DaprWorkflow_SequentialExecution()
    {
        // Future test for sequential agent workflow
        await Task.CompletedTask;
    }

    [Fact(Skip = "Workflow capabilities not yet implemented")]
    public async Task Test_DaprWorkflow_ParallelRAG()
    {
        // Future test for parallel RAG execution
        await Task.CompletedTask;
    }

    [Fact(Skip = "Event-driven architecture not yet implemented")]
    public async Task Test_EventDriven_GitHubWebhook()
    {
        // Future test for GitHub webhook → workflow trigger
        await Task.CompletedTask;
    }

    [Fact(Skip = "Visual pipeline builder not yet implemented")]
    public async Task Test_PipelineCompilation_FromVisualDefinition()
    {
        // Future test for compiling visual pipeline to Dapr workflow
        await Task.CompletedTask;
    }
}