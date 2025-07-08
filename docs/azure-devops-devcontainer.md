# Connecting Azure DevOps to DevContainer

## Option 1: GitHub Codespaces (Recommended)

1. **Create Codespace**:
   - Go to your GitHub repo
   - Click "Code" → "Codespaces" → "Create codespace on main"

2. **Start the API**:
   ```bash
   dotnet run --project src/Care.Ai.AppHost
   ```

3. **Get Public URL**:
   - Go to "Ports" tab in terminal panel
   - Find port 5000
   - Click the globe icon to make it public
   - Copy the URL

4. **Configure Azure DevOps**:
   - Project Settings → Service Hooks → Web Hooks
   - URL: `https://[your-codespace]-5000.app.github.dev/api/webhook/azure`

## Option 2: VS Code with Dev Tunnels

1. **Open in DevContainer**:
   ```bash
   # Clone repo
   git clone [your-repo]
   cd Care.Ai
   
   # Open in VS Code
   code .
   ```
   - F1 → "Dev Containers: Reopen in Container"

2. **Install Dev Tunnel in Container**:
   ```bash
   curl -sL https://aka.ms/DevTunnelCliInstall | bash
   devtunnel user login
   ```

3. **Create and Start Tunnel**:
   ```bash
   # Create tunnel
   devtunnel create care-ai --allow-anonymous
   
   # Start API
   dotnet run --project src/Care.Ai.AppHost &
   
   # Start tunnel
   devtunnel host care-ai -p 5000
   ```

4. **Configure Azure DevOps**:
   - Use the tunnel URL provided

## Option 3: VS Code Remote Tunnel

1. **Enable Remote Tunnel**:
   - Install "Remote - Tunnels" extension
   - F1 → "Remote-Tunnels: Turn on Remote Tunnel Access"
   - Sign in with GitHub/Microsoft account

2. **Access via Browser**:
   - Go to https://vscode.dev/tunnel/[your-machine-name]
   - Or use the URL shown in VS Code

## Testing the Webhook

```bash
# Test from inside devcontainer
curl -X POST http://localhost:5000/api/webhook/azure \
  -H "Content-Type: application/json" \
  -d '{
    "eventType": "git.pullrequest.created",
    "resource": {
      "pullRequestId": 123,
      "title": "Test PR",
      "description": "Test description"
    }
  }'
```

## Security Notes

- Codespaces URLs require GitHub authentication by default
- Dev Tunnels can be configured for anonymous or authenticated access
- Always use HTTPS URLs for production webhooks
- Consider adding webhook signature validation