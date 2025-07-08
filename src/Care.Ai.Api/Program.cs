using Care.Ai.Kernel.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers().AddDapr();

// Add Care.Ai.Kernel
builder.Services.AddCareAiKernel(builder.Configuration);

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCloudEvents();
app.MapControllers();
app.MapSubscribeHandler();

await app.RunAsync();