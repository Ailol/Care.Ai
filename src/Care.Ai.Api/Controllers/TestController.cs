using Dapr.Actors;
using Dapr.Actors.Client;
using Care.AI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Care.AI.Api.Controllers;

[ApiController]
[Route("test")]
public class TestController(IActorProxyFactory actorProxyFactory) : ControllerBase
{
    [HttpGet]
    public IActionResult Test()
    {
        return Ok("Test successful");
    }
    
    [HttpPost("actor")]
    public async Task<IActionResult> TestActor()
    {
        var actorId = new ActorId(Guid.NewGuid().ToString());
        var actor = actorProxyFactory.CreateActorProxy<IChatService>(
            actorId,
            "ChatService");
        
        var result = await actor.Chat("Test message", "session1");
        return Ok(result);
    }
}