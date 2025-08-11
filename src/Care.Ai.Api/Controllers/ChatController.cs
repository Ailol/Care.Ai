using Care.AI.Services;
using Microsoft.AspNetCore.Mvc;
using Dapr.Actors;
using Dapr.Actors.Client;

namespace Care.AI.Api.Controllers;

[ApiController]
[Route("chat")]
public class ChatController(IActorProxyFactory actorProxyFactory) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        try
        {
            var actor = actorProxyFactory.CreateActorProxy<IChatService>(
                new ActorId(request.UserId),
                "ChatService");
            
            var response = await actor.Chat(
                request.Message,
                request.SessionId ?? "default");
            
            return Ok(new { response });
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: 500,
                title: "Chat Error"
            );
        }
    }
}

public record ChatRequest(string UserId, string Message, string? SessionId);