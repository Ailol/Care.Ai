using Care.Ai.Frameworks.Rag;
using Care.Ai.Frameworks.SemanticKernel;
using Care.Ai.Kernel;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddHttpClient();

// Add Care.Ai.Kernel with multiple framework providers
builder.Services.AddCareAiKernel(builder.Configuration, kernelBuilder =>
{
    kernelBuilder.ExposeController = false; // Disable auto-controller since we have a manual one
    // Register SemanticKernel for agent orchestration
  
    
    // Register RAG framework for RAG processing
    kernelBuilder.AddFramework<RagFramework>(config =>
    {
        config.ServiceId = "rag-framework";
        config.Options["VectorStoreProvider"] = builder.Configuration["VectorStore:Provider"] 
            ?? throw new InvalidOperationException("VectorStore:Provider configuration is required");
        config.Options["VectorStoreUrl"] = builder.Configuration["VectorStore:Url"] 
            ?? throw new InvalidOperationException("VectorStore:Url configuration is required");
        config.Options["EmbeddingModel"] = builder.Configuration["Ollama-embedding:Model"] 
            ?? throw new InvalidOperationException("Ollama-embedding:Model configuration is required");
        config.Options["EmbeddingEndpoint"] = builder.Configuration["Ollama-embedding:Endpoint"] 
            ?? throw new InvalidOperationException("Ollama-embedding:Endpoint configuration is required");
        config.Options["RankerModel"] = builder.Configuration["Ollama-ranker:Model"] 
            ?? throw new InvalidOperationException("Ollama-ranker:Model configuration is required");
        config.Options["RankerEndpoint"] = builder.Configuration["Ollama-ranker:Endpoint"] 
            ?? throw new InvalidOperationException("Ollama-ranker:Endpoint configuration is required");
    });
    kernelBuilder.AddFramework<SemanticKernelFramework>(config =>
    {
        config.ServiceId = "semantic-kernel-local";
        config.Options["Model"] = builder.Configuration["Ollama:Model"]
            ?? throw new InvalidOperationException("Ollama:Model configuration is required");
        config.Options["Endpoint"] = builder.Configuration["Ollama:Endpoint"]
            ?? throw new InvalidOperationException("Ollama:Endpoint configuration is required");
    });
});

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapControllers();

await app.RunAsync();
