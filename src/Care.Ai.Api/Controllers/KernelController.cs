using Care.Ai.Kernel.Abstractions;
using Care.Ai.Kernel.Models;
using Microsoft.AspNetCore.Mvc;

namespace Care.Ai.Api.Controllers;

/// <summary>
/// Main API controller for AI kernel operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class KernelController : ControllerBase
{
    private readonly IKernel _kernel;
    private readonly ILogger<KernelController> _logger;

    public KernelController(IKernel kernel, ILogger<KernelController> logger)
    {
        _kernel = kernel;
        _logger = logger;
    }

    [HttpPost("execute")]
    public async Task<ActionResult<IResponse>> Execute(
        [FromBody] Request request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _kernel.ExecuteAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
#pragma warning disable CA1848 // Use the LoggerMessage delegates
            _logger.LogError(ex, "Error executing kernel request");
#pragma warning restore CA1848 // Use the LoggerMessage delegates
            return StatusCode(500, new Response
            {
                Success = false,
                ErrorMessage = "Internal server error"
            });
        }
    }

    [HttpGet("providers")]
    public async Task<ActionResult<IReadOnlyList<IKernelProviderManifest>>> GetProviders(
        CancellationToken cancellationToken = default)
    {
        var providers = await _kernel.GetProvidersAsync(cancellationToken);
        return Ok(providers);
    }

    [HttpGet("capabilities")]
    public async Task<ActionResult<object>> GetCapabilities(
        CancellationToken cancellationToken = default)
    {
        var providers = await _kernel.GetProvidersAsync(cancellationToken);
        
        // Aggregate all unique operations across all providers
        var operations = providers
            .SelectMany(p => p.Operations)
            .Distinct()
            .OrderBy(o => o)
            .ToList();
        
        // Aggregate all unique models across all providers
        var models = providers
            .SelectMany(p => p.AvailableModels)
            .Distinct()
            .OrderBy(m => m)
            .ToList();
        
        // Return unified view without provider details
        return Ok(new
        {
            availableOperations = operations,
            availableModels = models,
            totalProviders = providers.Count
        });
    }
}
