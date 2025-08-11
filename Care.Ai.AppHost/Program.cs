using Aspire.Hosting;
using CommunityToolkit.Aspire.Hosting.Dapr;

var builder = DistributedApplication.CreateBuilder(args);

// Add Dapr
builder.AddDapr();

// Add Redis for state store
var redis = builder.AddRedis("redis");

// RabbitMQ removed - using in-memory pubsub for development

// Add Dapr placement service as a container
var placement = builder.AddContainer("placement", "daprio/dapr")
    .WithEndpoint(port: 50005, targetPort: 50005, name: "grpc")
    .WithArgs("./placement", "--port", "50005")
    .WithVolume("dapr-placement-data", "/tmp/placement");


// Add Dapr scheduler service as a container (in-memory mode for development)
var scheduler = builder.AddContainer("dapr-scheduler", "daprio/dapr")
    .WithEndpoint(port: 6060, targetPort: 6060, name: "grpc")
    .WithArgs("./scheduler", "--port", "6060", "--etcd-data-dir", "/tmp");

// Add Care.Ai.Api with Dapr
var api = builder.AddProject<Projects.Care_AI_Api>("care-ai-api")
    .WithReference(redis)
    .WaitFor(redis)
    .WaitFor(placement)
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppPort = 5001,
        DaprHttpPort = 3500,
        DaprGrpcPort = 50001,
        ResourcesPaths = [Path.Combine(Directory.GetCurrentDirectory(), "components")],
        Config = Path.Combine(Directory.GetCurrentDirectory(), "dapr-config.yaml"),
        PlacementHostAddress = "localhost:50005",
        SchedulerHostAddress = "localhost:6060"
    });

await builder.Build().RunAsync();
