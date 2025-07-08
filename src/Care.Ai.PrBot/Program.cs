using Care.Ai.Kernel.Abstractions;
using Care.Ai.Kernel.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IKernel>(sp => 
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new KernelProxy(httpClient, builder.Configuration["CareAI:Url"] ?? "http://localhost:51353");
});

var app = builder.Build();

// GitHub webhook endpoint
app.MapPost("/webhook/github", async (GitHubWebhook webhook, IKernel kernel, HttpClient http) =>
{
    if (webhook.Action is "opened" or "synchronize")
    {
        // Get PR details
        var pr = webhook.PullRequest;
        var diff = await http.GetStringAsync(pr.DiffUrl);
        
        // Analyze with Care.AI
        var response = await kernel.ExecuteAsync(new Request
        {
            Type = "chat-completion",
            Data = new()
            {
                ["messages"] = new[]
                {
                    new { role = "system", content = "You are a code reviewer. Analyze the changes and write a concise PR description with key changes and potential impacts." },
                    new { role = "user", content = $"PR Title: {pr.Title}\n\nChanges:\n{diff}" }
                }
            }
        });
        
        // Update PR
        var token = builder.Configuration["GitHub:Token"];
        http.DefaultRequestHeaders.Add("Authorization", $"token {token}");
        
        await http.PatchAsJsonAsync(pr.Url, new
        {
            body = $"{pr.Body}\n\n## 🤖 AI Analysis\n{response.Result}\n\n*Updated by Care.AI PR Bot*"
        });
    }
});

// Azure DevOps webhook
app.MapPost("/webhook/azure", async (AzureWebhook webhook, IKernel kernel, HttpClient http) =>
{
    if (webhook.EventType == "git.pullrequest.created" || webhook.EventType == "git.pullrequest.updated")
    {
        var pr = webhook.Resource;
        
        // Get diff
        var pat = builder.Configuration["Azure:PAT"];
        http.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{pat}"))}");
        
        var diffUrl = $"{pr.Repository.Url}/diffs?baseVersion={pr.TargetRefName}&targetVersion={pr.SourceRefName}";
        var diff = await http.GetStringAsync(diffUrl);
        
        // Analyze
        var response = await kernel.ExecuteAsync(new Request
        {
            Type = "chat-completion",
            Data = new()
            {
                ["messages"] = new[]
                {
                    new { role = "user", content = $"Review this PR and suggest a description:\n{diff}" }
                }
            }
        });
        
        // Update PR
        await http.SendAsync(new HttpRequestMessage(new HttpMethod("PATCH"), pr.Url)
        {
            Content = JsonContent.Create(new { description = $"{pr.Description}\n\n## AI Review\n{response.Result}" })
        });
    }
});

app.MapGet("/", () => "Care.AI PR Bot is running! Configure webhooks to /webhook/github or /webhook/azure");

app.Run("http://localhost:5555");

// Minimal types
record GitHubWebhook(string Action, PR PullRequest);
record PR(string Title, string Body, string Url, string DiffUrl);
record AzureWebhook(string EventType, AzurePR Resource);
record AzurePR(string Description, string Url, Repo Repository, string SourceRefName, string TargetRefName);
record Repo(string Url);

class KernelProxy : IKernel
{
    private readonly HttpClient _http;
    private readonly string _url;
    
    public KernelProxy(HttpClient http, string url)
    {
        _http = http;
        _url = url;
    }
    
    public async Task<IResponse> ExecuteAsync(IRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync($"{_url}/api/kernel/execute", request, ct);
        return await response.Content.ReadFromJsonAsync<Response>(ct) ?? new Response { Success = false };
    }
    
    public Task<IReadOnlyList<IKernelProviderManifest>> GetProvidersAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<IKernelProviderManifest>>(new List<IKernelProviderManifest>());
}