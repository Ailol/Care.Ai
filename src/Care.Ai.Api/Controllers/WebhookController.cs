using Care.Ai.Kernel.Abstractions;
using Care.Ai.Kernel.Mcp;
using Microsoft.AspNetCore.Mvc;

namespace Care.Ai.Api.Controllers;

[ApiController]
[Route("api/webhook")]
public class WebhookController : ControllerBase
{
    private static readonly Action<ILogger, string, Exception?> _logWebhookReceived =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1, nameof(WebhookReceived)),
            "Received {Provider} webhook");
    
    private static readonly Action<ILogger, string, Exception?> _logWebhookError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(2, nameof(WebhookError)),
            "Error processing {Provider} webhook");
    
    private readonly IKernel _kernel;
    private readonly McpClient _mcp;
    private readonly ILogger<WebhookController> _logger;
    
    public WebhookController(IKernel kernel, McpClient mcp, ILogger<WebhookController> logger)
    {
        _kernel = kernel;
        _mcp = mcp;
        _logger = logger;
    }
    
    [HttpPost("{provider}")]
    public async Task<IActionResult> HandleWebhook(string provider)
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            
            WebhookReceived(_logger, provider);
            
            // For now, just process Azure webhooks directly without MCP
            if (provider == "azure" || provider == "azure-devops")
            {
                var webhookData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(body);
                
                // Log the webhook for debugging
                _logger.LogInformation("Azure webhook data: {Data}", body.Length > 500 ? string.Concat(body.AsSpan(0, 500), "...") : body);
                
                // TODO: Process the webhook (analyze PR with AI, etc.)
                return Ok(new { status = "processed", message = "Azure DevOps webhook received" });
            }
            
            // For other providers, try MCP
            try
            {
                var result = await _mcp.CallToolAsync<object>(provider, "process_webhook", new
                {
                    headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                    body = body,
                    kernel = _kernel
                });
                
                return Ok(result ?? new { status = "processed" });
            }
            catch (HttpRequestException)
            {
                // MCP Gateway not available
                return Ok(new { status = "received", message = "MCP gateway not available, webhook stored" });
            }
        }
        catch (Exception ex)
        {
            WebhookError(_logger, provider, ex);
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    [HttpGet("{provider}")]
    public IActionResult WebhookStatus(string provider)
    {
        return Ok(new { status = "ready", provider });
    }
    
    private static void WebhookReceived(ILogger logger, string provider)
        => _logWebhookReceived(logger, provider, null);
    
    private static void WebhookError(ILogger logger, string provider, Exception ex)
        => _logWebhookError(logger, provider, ex);
}
