using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDaprClient();

var app = builder.Build();

// Provider manifest endpoint
app.MapGet("/manifest", () => new
{
    ProviderId = "mock-provider",
    Name = "Mock AI Provider",
    Version = "1.0.0",
    Operations = new[] { "text-completion", "chat-completion", "embedding" },
    AvailableModels = new[] { "mock-gpt", "mock-embed" },
    Priority = 100
});

// Execute endpoint
app.MapPost("/execute", async ([FromBody] ExecuteRequest request) =>
{
    await Task.Delay(100); // Simulate processing
    
    return new
    {
        Success = true,
        Result = request.Type switch
        {
            "text-completion" => new { text = $"Mock response to: {request.Data["prompt"]}" },
            "chat-completion" => new { message = "Mock chat response" },
            "embedding" => new { embeddings = new[] { 0.1, 0.2, 0.3 } },
            _ => null
        },
        Metadata = new Dictionary<string, object>
        {
            ["provider"] = "mock-provider",
            ["model"] = request.Data.ContainsKey("model") ? request.Data["model"] : "mock-gpt",
            ["processingTime"] = "100ms"
        }
    };
});

app.MapGet("/health", () => "Healthy");

app.Run();

record ExecuteRequest(string Type, Dictionary<string, object> Data);