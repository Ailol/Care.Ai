using Aspire.Hosting;
using Aspire.Hosting.Dapr;
using Aspire.Hosting.Redis;
using System.Collections.Immutable;

var builder = DistributedApplication.CreateBuilder(args);

// Disable dashboard if environment variables are not set
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.Configuration["ASPIRE:DisableDashboard"] = "true";
}

// Add Redis for Dapr state store
var redis = builder.AddRedis("redis", port: 6379);

// Add RabbitMQ for both MessageBus and Dapr pubsub
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

// Add Dapr components - using default Redis for both
var statestore = builder.AddDaprStateStore("statestore");
var pubsub = builder.AddDaprPubSub("pubsub");

// Configure Dapr sidecar options for API
DaprSidecarOptions apiSidecarOptions = new()
{
    AppId = "care-ai-api",
    AppPort = 51353, // HTTP port from launchSettings.json
    AppProtocol = "http",
    DaprGrpcPort = 50001,
    DaprHttpPort = 50002,
    MetricsPort = 50003,
    //EnableActors = false // Disable actors since we don't use them
};

// Configure Dapr sidecar options for MCP
DaprSidecarOptions mcpSidecarOptions = new()
{
    AppId = "care-ai-mcp",
    AppPort = 51352, // HTTP port from launchSettings.json
    AppProtocol = "http",
    DaprGrpcPort = 50011,
    DaprHttpPort = 50012,
    MetricsPort = 50013,
    //EnableActors = false // Disable actors since we don't use them
};

// Add Care.Ai.Api with Dapr sidecar
var api = builder.AddProject<Projects.Care_Ai_Api>("care-ai-api")
    .WithReference(rabbitmq)
    .WithReference(redis)
    .WithDaprSidecar(apiSidecarOptions)
    .WithReference(statestore)
    .WithReference(pubsub);

// Add Care.Ai.MCP with Dapr sidecar
var mcp = builder.AddProject<Projects.Care_Ai_MCP>("care-ai-mcp")
    .WithReference(rabbitmq)
    .WithReference(redis)
    .WithDaprSidecar(mcpSidecarOptions)
    .WithReference(statestore)
    .WithReference(pubsub);

// Configure Dapr sidecar options for Semantic Kernel
DaprSidecarOptions skSidecarOptions = new()
{
    AppId = "semantic-kernel-provider",
    AppPort = 5050, // Default port for SK provider
    AppProtocol = "http",
    DaprGrpcPort = 50021,
    DaprHttpPort = 50022,
    MetricsPort = 50023,
    // Components are configured via Aspire, not files
};

// Add Semantic Kernel provider with Dapr sidecar
// Note: Ollama configuration is in appsettings.json
var semanticKernel = builder.AddProject<Projects.Care_Ai_Providers_SemanticKernel>(
    "semantic-kernel-provider")
    .WithReference(rabbitmq)
    .WithDaprSidecar(skSidecarOptions)
    .WithReference(statestore)
    .WithReference(pubsub);

// Configure Dapr sidecar options for LlamaIndex
DaprSidecarOptions liSidecarOptions = new()
{
    AppId = "llamaindex-provider",
    AppPort = 5051, // Different port from SK provider
    AppProtocol = "http",
    DaprGrpcPort = 50031,
    DaprHttpPort = 50032,
    MetricsPort = 50033,
    // Components are configured via Aspire, not files
};

// Add LlamaIndex provider as Docker container with Dapr sidecar
var dockerfileContext = Path.Combine("..", "providers", "care-ai-providers-llamaindex");
var llamaIndex = builder.AddDockerfile("llamaindex-provider", dockerfileContext)
    .WithHttpEndpoint(port: 5051, targetPort: 5051, name: "http")
    .WithEnvironment("OLLAMA_ENDPOINT", "http://host.docker.internal:11434")
    .WithEnvironment("OLLAMA_MODEL", "qwen3-clean:latest")
    .WithReference(rabbitmq)
    .WithDaprSidecar(liSidecarOptions)
    .WithReference(statestore)
    .WithReference(pubsub);

await builder.Build().RunAsync();
