using Care.Ai.Kernel.Abstractions;
using Care.Ai.Kernel.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Care.Ai.Api.Controllers;

[ApiController]
[Route("v1")]
public class OpenAIController : ControllerBase
{
    private readonly IKernel _kernel;
    
    public OpenAIController(IKernel kernel) => _kernel = kernel;

    [HttpPost("chat/completions")]
    public async Task<IActionResult> ChatCompletions([FromBody] JsonElement request)
    {
        // Extract essentials from OpenAI format
        var messages = request.GetProperty("messages");
        var model = request.TryGetProperty("model", out var m) ? m.GetString() : null;
        var stream = request.TryGetProperty("stream", out var s) && s.GetBoolean();
        
        // Execute via kernel
        var response = await _kernel.ExecuteAsync(new Request
        {
            Type = "chat-completion",
            Data = new()
            {
                ["messages"] = messages,
                ["model"] = model ?? "qwen3-clean:latest"
            }
        });
        
        // Extract the actual message content
        var content = "";
        if (response.Result is Dictionary<string, object> resultDict)
        {
            content = resultDict.GetValueOrDefault("message")?.ToString() ?? "";
        }
        else if (response.Result != null)
        {
            content = response.Result.ToString() ?? "";
        }
        
        // Handle streaming if requested
        if (stream)
        {
            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");
            
            // Send the response as a stream
            var streamData = new
            {
                id = $"chatcmpl-{Guid.NewGuid()}",
                @object = "chat.completion.chunk",
                created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                model = model ?? "care-ai",
                choices = new[]
                {
                    new
                    {
                        index = 0,
                        delta = new
                        {
                            content = content
                        },
                        finish_reason = (string?)null
                    }
                }
            };
            
            await Response.WriteAsync($"data: {JsonSerializer.Serialize(streamData)}\n\n");
            
            // Send done signal
            var doneData = new
            {
                id = $"chatcmpl-{Guid.NewGuid()}",
                @object = "chat.completion.chunk",
                created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                model = model ?? "care-ai",
                choices = new[]
                {
                    new
                    {
                        index = 0,
                        delta = new { content = string.Empty },
                        finish_reason = "stop"
                    }
                }
            };
            
            await Response.WriteAsync($"data: {JsonSerializer.Serialize(doneData)}\n\n");
            await Response.WriteAsync("data: [DONE]\n\n");
            await Response.Body.FlushAsync();
            
            return new EmptyResult();
        }
        
        // Non-streaming response
        return Ok(new
        {
            id = $"chatcmpl-{Guid.NewGuid()}",
            @object = "chat.completion",
            created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            model = model ?? "care-ai",
            choices = new[]
            {
                new
                {
                    index = 0,
                    message = new
                    {
                        role = "assistant",
                        content = content
                    },
                    finish_reason = "stop"
                }
            },
            usage = new
            {
                prompt_tokens = 0,
                completion_tokens = 0,
                total_tokens = 0
            }
        });
    }

    [HttpGet("models")]
    public async Task<IActionResult> Models()
    {
        var providers = await _kernel.GetProvidersAsync();
        var models = providers.SelectMany(p => p.AvailableModels).Distinct();
        
        return Ok(new
        {
            @object = "list",
            data = models.Select(m => new
            {
                id = m,
                @object = "model",
                created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                owned_by = "care-ai",
                permission = Array.Empty<object>(),
                root = m,
                parent = (string?)null
            }).ToArray()
        });
    }
}
