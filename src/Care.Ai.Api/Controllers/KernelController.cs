using Care.AI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Dapr.Actors;
using Dapr.Actors.Client;

namespace Care.AI.Api.Controllers;

[ApiController]
[Route("execute")]
[Authorize] // Require authentication
public class KernelController(IActorProxyFactory actorProxyFactory) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Execute([FromBody] ExecuteRequest request)
    {
        var actor = actorProxyFactory.CreateActorProxy<IKernelService>(
            new ActorId(request.UserId),
            "KernelService");
        
        // Convert args to Dictionary<string, object> if needed
        Dictionary<string, object>? arguments = null;
        if (request.Args != null)
        {
            if (request.Args is System.Text.Json.JsonElement jsonElement && 
                jsonElement.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                arguments = new Dictionary<string, object>();
                foreach (var property in jsonElement.EnumerateObject())
                {
                    arguments[property.Name] = property.Value.ToString();
                }
            }
            else if (request.Args is IDictionary<string, object> dict)
            {
                arguments = new Dictionary<string, object>(dict);
            }
            else
            {
                arguments = new Dictionary<string, object> { ["input"] = request.Args };
            }
        }
        
        var kernelRequest = new KernelRequest(request.Function, arguments);
        var result = await actor.Execute(kernelRequest);
        
        return Ok(result);
    }
}

public record ExecuteRequest(string UserId, string Function, object Args);