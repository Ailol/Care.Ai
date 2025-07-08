using Microsoft.AspNetCore.Mvc;

namespace Care.Ai.Api.Controllers;

[ApiController]
[Route("api/mcp")]
public class McpController : ControllerBase
{
    private static readonly Dictionary<string, McpServer> _servers = new();
    
    [HttpPost("register")]
    public IActionResult Register([FromBody] McpServer server)
    {
        _servers[server.Id] = server;
        return Ok(new { message = $"Registered MCP server: {server.Id}" });
    }
    
    [HttpGet("servers")]
    public IActionResult ListServers()
    {
        return Ok(_servers.Values);
    }

    [HttpPost("{serverId}/call")]
    public IActionResult CallTool(string serverId, [FromBody] McpCall call)
    {
        if (!_servers.TryGetValue(serverId, out var server))
            return NotFound($"MCP server '{serverId}' not found");

        // This would call the actual MCP server
        // For now, just return a placeholder
        return Ok(new { result = "Called " + call.Tool });
    }
}

public record McpServer(string Id, string Name, string Type, List<string> Tools);
public record McpCall(string Tool, object Parameters);
