# Care.Ai.Kernel

A unified AI kernel gateway that routes requests to multiple AI orchestration platforms.

## 🚀 Quick Start with GitHub Codespaces

### 1. Create a Codespace
[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/your-username/Care.Ai)

Or manually:
- Click the green **Code** button above
- Select **Codespaces** tab
- Click **Create codespace on main**

### 2. Wait for Setup
The Codespace will automatically:
- Install .NET 9 and Aspire workload
- Set up Docker-in-Docker for Aspire containers
- Build Care.AI packages
- Configure HTTPS certificates

### 3. Run the Application
Once setup completes (3-5 minutes):

```bash
dotnet run --project src/Care.Ai.AppHost
```

The Aspire Dashboard will open automatically at port 15888.

### 3. Get Your Webhook URL
1. Go to the **PORTS** tab in the terminal panel
2. Find port **5000** (labeled "API - Azure DevOps Webhook")
3. Click the copy icon to get your public URL
4. Add `/api/webhook/azure` to the URL

Your webhook URL will look like:
```
https://username-careai-randomstring-5000.app.github.dev/api/webhook/azure
```

### 4. Configure Azure DevOps
1. In Azure DevOps: Project Settings → Service Hooks
2. Create a new Web Hook subscription
3. Event: Pull Request Created/Updated
4. URL: Your codespace webhook URL from step 3
5. Test with a new PR!

## 📁 Project Structure

```
Care.Ai.Kernel/
├── packages/              # NuGet packages
│   ├── Care.Ai.Kernel.Abstractions/
│   └── Care.Ai.Kernel/
├── src/
│   ├── Care.Ai.Api/      # Main API with webhook handler
│   ├── Care.Ai.AppHost/  # Aspire orchestration
│   └── providers/        # AI provider implementations
└── .devcontainer/        # Codespaces configuration
```

## 🛠️ Local Development

### Prerequisites
- .NET 9 SDK
- Docker

### Building
```bash
# Build packages
./build-packages.sh

# Run with Aspire
dotnet run --project src/Care.Ai.AppHost
```

## 🔗 Webhook Integration

The API automatically handles Azure DevOps webhooks at `/api/webhook/azure`:

```csharp
// Webhook controller handles PR events
[HttpPost("azure")]
public async Task<IActionResult> HandleAzureWebhook()
{
    // Receives PR webhook
    // Generates AI description
    // Updates PR via Azure DevOps API
}
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## 📝 License

[Your License Here]