var builder = DistributedApplication.CreateBuilder(args);

// Add Redis with fixed port
var redis = builder.AddRedis("redis", port: 6379);

// Add RabbitMQ with fixed ports
var rabbitmq = builder.AddRabbitMQ("rabbitmq", port: 5672)
    .WithManagementPlugin(port: 15672);

// Add Care.Ai.Api - the main gateway API
var api = builder.AddProject<Projects.Care_Ai_Api>("care-ai-api")
    .WithReference(rabbitmq)
    .WithReference(redis);

// Optionally add LlamaIndex provider as Docker container if it exists
var dockerfileContext = Path.Combine("..", "src", "providers", "care-ai-providers-llamaindex");
if (Directory.Exists(dockerfileContext))
{
    var llamaIndex = builder.AddDockerfile("llamaindex-provider", dockerfileContext)
        .WithHttpEndpoint(port: 5051, targetPort: 5051)
        .WithEnvironment("OLLAMA_ENDPOINT", "http://host.docker.internal:11434")
        .WithEnvironment("OLLAMA_MODEL", "qwen3-clean:latest")
        .WithReference(rabbitmq);
}

await builder.Build().RunAsync();
