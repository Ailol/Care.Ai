using Care.Ai.Kernel.Abstractions;
using Care.Ai.Kernel.Models;
using Microsoft.AspNetCore.Mvc;

namespace SampleProvider.Controllers;

/// <summary>
/// Sample provider implementation
/// </summary>
[ApiController]
[Route("")]
public class ProviderController : ControllerBase
{
    [HttpGet("manifest")]
    public ActionResult<KernelProviderManifest> GetManifest()
    {
        return new KernelProviderManifest
        {
            ProviderId = "sample-provider",
            Version = "1.0.0",
            ServiceId = "sample-provider",
            ExecuteEndpoint = "/execute",
            Priority = 100,
            AvailableModels = new() { "gpt-3.5-turbo", "gpt-4" },
            Operations = new() { "chat", "completion" }
        };
    }

    [HttpPost("execute")]
    public async Task<ActionResult<Response>> Execute(
        [FromBody] Request request)
    {
        // Your AI logic here
        await Task.Delay(100); // Simulate work
        
        return new Response
        {
            Success = true,
            Result = $"Response from sample provider for {request.Type}"
        };
    }
}