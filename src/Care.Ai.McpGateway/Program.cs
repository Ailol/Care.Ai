var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// MCP servers registry (could be Redis, DB, etc.)
var mcpServers = new Dictionary<string, string>();

// Register MCP server
app.MapPost("/mcp/register", (McpRegistration reg) =>
{
    mcpServers[reg.Id] = reg.Url;
    return Results.Ok(new { message = $"Registered {reg.Id}" });
});

// List available MCP servers
app.MapGet("/mcp/servers", () => mcpServers);

// Proxy to any MCP server
app.MapPost("/mcp/{serverId}/tool/{toolName}", async (string serverId, string toolName, HttpContext context) =>
{
    if (!mcpServers.TryGetValue(serverId, out var serverUrl))
        return Results.NotFound($"MCP server '{serverId}' not found");
    
    // Forward the request to the actual MCP server
    using var client = new HttpClient();
    var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
    
    var response = await client.PostAsync($"{serverUrl}/tool/{toolName}", 
        new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
    
    var result = await response.Content.ReadAsStringAsync();
    return Results.Content(result, "application/json");
});

app.Run();

sealed record McpRegistration(string Id, string Name, string Url);