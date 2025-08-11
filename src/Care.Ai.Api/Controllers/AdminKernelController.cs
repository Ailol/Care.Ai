using Care.AI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Dapr.Actors;
using Dapr.Actors.Client;

namespace Care.AI.Api.Controllers;

[ApiController]
[Route("admin/execute")]
[Authorize(Roles = "Administrator")] // Only administrators can access
public class AdminKernelController(IActorProxyFactory actorProxyFactory) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Execute([FromBody] ExecuteRequest request)
    {
        // Only allow execution of Admin plugin functions
        if (!request.Function.StartsWith("Admin.", StringComparison.Ordinal))
        {
            return Forbid("Only Admin plugin functions are allowed through this endpoint");
        }
        
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

public record AdminExecuteRequest(string UserId, string Function, object Args);