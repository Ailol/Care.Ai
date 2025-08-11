using Care.AI;
using Care.AI.Api.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Configure to listen on port 5001
builder.WebHost.UseUrls("http://localhost:5001");

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDaprClient();

// Add authentication (using mock/dev authentication for demo)
// In production, replace with real authentication (JWT, Azure AD, etc.)
builder.Services.AddAuthentication("MockScheme")
    .AddScheme<MockAuthenticationSchemeOptions, MockAuthenticationHandler>("MockScheme", null);
builder.Services.AddAuthorization();

builder.Services.AddCareAI();

// Configure kernel with role-based plugins
builder.Services.AddSingleton<Care.AI.Api.Plugins.AdminPlugin>();
builder.Services.AddSingleton<Care.AI.Api.Plugins.UserPlugin>();
builder.Services.AddSingleton<Care.AI.Api.Plugins.TestPlugin>();

// Plugins will be registered in InitializeKernelProvider

var app = builder.Build();

// Initialize the kernel provider
Care.AI.Extensions.InitializeKernelProvider(app.Services);

// Register plugins after kernel is initialized
var kernel = Care.AI.KernelProvider.GetKernel();
var adminPlugin = app.Services.GetRequiredService<Care.AI.Api.Plugins.AdminPlugin>();
var userPlugin = app.Services.GetRequiredService<Care.AI.Api.Plugins.UserPlugin>();
var testPlugin = app.Services.GetRequiredService<Care.AI.Api.Plugins.TestPlugin>();

kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(adminPlugin, "Admin"));
kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(userPlugin, "User"));
kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(testPlugin, "TestPlugin"));

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseCloudEvents();
app.MapSubscribeHandler();
app.MapActorsHandlers();

app.MapControllers();

app.Run();
