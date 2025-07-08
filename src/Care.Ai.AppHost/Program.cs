var builder = DistributedApplication.CreateBuilder(args);

// Add Redis
var redis = builder.AddRedis("redis");

// Add RabbitMQ
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

// Add Care.Ai.Api
var api = builder.AddProject<Projects.Care_Ai_Api>("care-ai-api")
    .WithReference(rabbitmq)
    .WithReference(redis);

// Add Semantic Kernel provider
var semanticKernel = builder.AddProject<Projects.Care_Ai_Providers_SemanticKernel>("semantic-kernel-provider")
    .WithReference(rabbitmq);

// Add LlamaIndex provider as Docker container
var dockerfileContext = Path.Combine("..", "providers", "care-ai-providers-llamaindex");
var llamaIndex = builder.AddDockerfile("llamaindex-provider", dockerfileContext)
    .WithHttpEndpoint(port: 5051, targetPort: 5051, name: "http")
    .WithEnvironment("OLLAMA_ENDPOINT", "http://host.docker.internal:11434")
    .WithEnvironment("OLLAMA_MODEL", "qwen3-clean:latest")
    .WithReference(rabbitmq);

// Add MCP Gateway
var mcpGateway = builder.AddProject<Projects.Care_Ai_McpGateway>("care-ai-mcpgateway");

// Update API to reference MCP Gateway and expose webhook endpoints
api.WithReference(mcpGateway)
   .WithEnvironment("McpGateway__Url", mcpGateway.GetEndpoint("http"))
   .WithExternalHttpEndpoints();

// Add MCP project
var mcp = builder.AddProject<Projects.Care_Ai_MCP>("care-ai-mcp")
    .WithReference(api);
await builder.Build().RunAsync();