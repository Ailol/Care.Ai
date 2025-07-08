using Care.Ai.Kernel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Care.Ai.Providers.SemanticKernel;

[ApiController]
[Route("/")]
public class KernelProviderController : ControllerBase
{
    private readonly Microsoft.SemanticKernel.Kernel _kernel;
    private readonly IConfiguration _configuration;
    private static readonly string[] Models = { "qwen3-clean:latest", "qwen3:0.6b" };
    
    public KernelProviderController(Microsoft.SemanticKernel.Kernel kernel, IConfiguration configuration)
    {
        _kernel = kernel;
        _configuration = configuration;
    }
    
    [HttpGet("manifest")]
    public IActionResult GetManifest() => Ok(new KernelProviderManifest
    {
        ProviderId = "semantic-kernel-provider",
        Version = "1.0.0",
        AvailableModels = Models.ToList(),
        Operations = new List<string> { "chat-completion" },
        ServiceId = _configuration["Dapr:AppId"] ?? "semantic-kernel-provider",
        ExecuteEndpoint = "/execute",
        Priority = 100,
        Skills = new Dictionary<string, object>
        {
            ["tech"] = "semantic-kernel",
            ["type"] = "orchestration",
            ["agents"] = true,
            ["microsoft"] = true
        }
    });
    
    [HttpPost("execute")]
    public async Task<IActionResult> Execute([FromBody] Request request)
    {
        try
        {
            if (request.Type != "chat-completion")
                return Ok(new Response { Success = false, ErrorMessage = "Only chat-completion supported" });
            
            var chatService = _kernel.GetRequiredService<IChatCompletionService>();
            var chatHistory = new ChatHistory();
            
            if (request.Data.TryGetValue("messages", out var msgs))
            {
                // Handle JsonElement array from System.Text.Json
                if (msgs is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var msg in jsonElement.EnumerateArray())
                    {
                        var role = msg.GetProperty("role").GetString();
                        var content = msg.GetProperty("content").GetString();
                        chatHistory.AddMessage(
                            role == "system" ? AuthorRole.System :
                            role == "assistant" ? AuthorRole.Assistant : 
                            AuthorRole.User, 
                            content ?? "");
                    }
                }
                // Handle IEnumerable for other scenarios
                else if (msgs is IEnumerable<object> messages)
                {
                    foreach (var msg in messages)
                        if (msg is Dictionary<string, object> m)
                            chatHistory.AddMessage(
                                m.GetValueOrDefault("role")?.ToString() == "system" ? AuthorRole.System :
                                m.GetValueOrDefault("role")?.ToString() == "assistant" ? AuthorRole.Assistant : 
                                AuthorRole.User, 
                                m.GetValueOrDefault("content")?.ToString() ?? "");
                }
            }
            
            var result = await chatService.GetChatMessageContentAsync(chatHistory);
            return Ok(new Response { Success = true, Result = new { message = result.Content, role = "assistant" } });
        }
        catch (Exception ex) { return Ok(new Response { Success = false, ErrorMessage = ex.Message }); }
    }
    
    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "Healthy" });
}
