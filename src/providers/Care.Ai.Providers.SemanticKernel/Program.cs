using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

#pragma warning disable SKEXP0010 // Type is for evaluation purposes only

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddDaprClient();
builder.Services.AddHttpClient();

// Configure Semantic Kernel with Ollama
builder.Services.AddSingleton<Microsoft.SemanticKernel.Kernel>(_ =>
{
    var kernelBuilder = Kernel.CreateBuilder();
    
    // Use Ollama's OpenAI-compatible endpoint
    var ollamaEndpoint = builder.Configuration["Ollama:Endpoint"] ?? "http://localhost:11434/v1";
    var model = builder.Configuration["Ollama:Model"] ?? "qwen3-clean:latest";
    
    // Custom endpoints are experimental in SK, but needed for Ollama
    // Ollama doesn't require API key, but SK expects one
    kernelBuilder.AddOpenAIChatCompletion(
        modelId: model,
        apiKey: "ollama", // Dummy key for Ollama
        endpoint: new Uri(ollamaEndpoint));
    
    return kernelBuilder.Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapControllers();

app.Run();
