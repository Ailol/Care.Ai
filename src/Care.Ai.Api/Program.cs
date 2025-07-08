using Care.Ai.Kernel.Extensions;
using Care.Ai.Kernel.Mcp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Add Care.Ai.Kernel
builder.Services.AddCareAiKernel(builder.Configuration);

// Add MCP Client
builder.Services.AddSingleton<McpClient>(sp => 
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var mcpUrl = builder.Configuration["McpGateway:Url"] ?? "http://localhost:5050";
    return new McpClient(httpClient, mcpUrl);
});

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapControllers();

await app.RunAsync();