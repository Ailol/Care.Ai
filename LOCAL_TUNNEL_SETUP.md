# Running Remote Tunnels from Local Machine

## Option 1: VS Code Remote Tunnels (Easiest - Built into VS Code)

### Setup (One-time)
```bash
# Install VS Code CLI if you don't have it
curl -Lk 'https://code.visualstudio.com/sha/download?build=stable&os=cli-alpine-x64' --output vscode_cli.tar.gz
tar -xf vscode_cli.tar.gz

# Or on Windows/Mac, just use the 'code' command that comes with VS Code
```

### Run Tunnel
```bash
# In your project directory
code tunnel

# First time: Sign in with GitHub or Microsoft account
# You'll get a URL like: https://vscode.dev/tunnel/YOUR-MACHINE-NAME/~/Care.Ai
```

### Start Your App
```bash
# In another terminal
dotnet run --project src/Care.Ai.AppHost

# The tunnel automatically forwards all ports!
```

Your webhook URL: `https://YOUR-MACHINE-NAME.tunnels.api.visualstudio.com:5000/api/webhook/azure`

## Option 2: Dev Tunnels CLI (Microsoft's Standalone Tool)

### Install
```bash
# Windows (PowerShell)
winget install Microsoft.devtunnel

# Mac/Linux
curl -sL https://aka.ms/DevTunnelCliInstall | bash
```

### Run
```bash
# One-time login
devtunnel user login

# Create persistent tunnel
devtunnel create care-ai --allow-anonymous

# Start your app
dotnet run --project src/Care.Ai.AppHost

# In another terminal, start tunnel
devtunnel host care-ai -p 5000
```

## Option 3: Cloudflare Tunnel (Free, No Login)

### Install
```bash
# Windows
winget install --id Cloudflare.cloudflared

# Mac
brew install cloudflared

# Linux
wget -q https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64.deb
sudo dpkg -i cloudflared-linux-amd64.deb
```

### Run
```bash
# Start your app
dotnet run --project src/Care.Ai.AppHost

# Start tunnel (no login needed!)
cloudflared tunnel --url http://localhost:5000
```

## Option 4: Bore (Simplest - No Install)

```bash
# Using cargo (Rust)
cargo install bore-cli

# Or download binary from: https://github.com/ekzhang/bore/releases

# Run
dotnet run --project src/Care.Ai.AppHost
bore local 5000 --to bore.pub
```

## Quick Comparison

| Tool | Login Required | Persistent URL | Install Size |
|------|---------------|----------------|--------------|
| VS Code Tunnel | Yes (GitHub/MS) | Yes | Already have it |
| Dev Tunnels | Yes (MS) | Yes | ~50MB |
| Cloudflare | No | No | ~30MB |
| Bore | No | No | ~5MB |

## Recommended: VS Code Tunnel

Since you're already using VS Code:

1. Just run `code tunnel` in your project
2. Sign in once with GitHub
3. You get a permanent URL that works every time
4. All ports are automatically forwarded
5. Works with your existing VS Code setup

## Security Note

For production webhooks, add authentication:
```csharp
// In your webhook controller
[HttpPost("azure")]
public async Task<IActionResult> HandleAzureWebhook(
    [FromHeader(Name = "X-Azure-Signature")] string signature)
{
    // Validate signature
    if (!ValidateWebhookSignature(signature, Request.Body))
        return Unauthorized();
    
    // Process webhook...
}
```