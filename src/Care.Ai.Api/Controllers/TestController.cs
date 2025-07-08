using Microsoft.AspNetCore.Mvc;

namespace Care.Ai.Api.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly IServiceProvider _services;
    
    public TestController(IServiceProvider services)
    {
        _services = services;
    }
    
    [HttpPost("simulate-webhook/{provider}")]
    public async Task<IActionResult> SimulateWebhook(string provider)
    {
        // Simulate an Azure DevOps PR webhook
        var testWebhook = new
        {
            subscriptionId = Guid.NewGuid().ToString(),
            notificationId = 1,
            eventType = "git.pullrequest.created",
            resource = new
            {
                pullRequestId = 123,
                title = "Test PR: Add new feature",
                description = "This is a test PR",
                sourceRefName = "refs/heads/feature/test",
                targetRefName = "refs/heads/main",
                url = $"https://dev.azure.com/test/project/_apis/git/repositories/repo/pullRequests/123",
                repository = new
                {
                    id = "test-repo",
                    name = "TestRepo",
                    project = new
                    {
                        id = "test-project",
                        name = "TestProject"
                    }
                }
            }
        };
        
        // Call the webhook endpoint internally
        var webhookController = _services.GetRequiredService<WebhookController>();
        
        // Simulate the request
        Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(
            System.Text.Json.JsonSerializer.Serialize(testWebhook)));
        Request.ContentType = "application/json";
        
        return await webhookController.HandleWebhook(provider);
    }
}