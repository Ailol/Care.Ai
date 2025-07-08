# GitHub Codespaces Quick Start

## 1. Create a Codespace (30 seconds)

Go to your GitHub repository and click:
- **Code** (green button)
- **Codespaces** tab
- **Create codespace on main**

## 2. Wait for Setup (2-3 minutes)

The devcontainer will automatically:
- Install .NET 9
- Build the Care.AI packages
- Restore all dependencies
- Set up port forwarding

## 3. Start the Application

In the terminal, run:
```bash
dotnet run --project src/Care.Ai.AppHost
```

## 4. Get Your Webhook URL

1. Click on the **PORTS** tab in the terminal panel
2. Find port **5000** (labeled "Care.AI API - Webhook Endpoint")
3. It should already show as **Public** with a globe icon
4. Click the **Copy** button to get your URL

Your webhook URL will be:
```
https://[your-username]-[repo-name]-[random-string]-5000.app.github.dev/api/webhook/azure
```

## 5. Configure Azure DevOps

1. Go to your Azure DevOps project
2. Project Settings → Service Hooks
3. Click **+ Create subscription**
4. Choose **Web Hooks**
5. Configure:
   - **Trigger**: Pull request created/updated
   - **URL**: Paste your Codespace URL from step 4
   - **Resource details**: Leave defaults
   - **Messages to send**: All
   - **Detailed messages**: Yes

## 6. Test It!

Create or update a PR in Azure DevOps. You should see:
- Webhook received in Codespace terminal
- PR description updated with AI-generated content

## Troubleshooting

**Port not public?**
- Click the lock icon next to port 5000
- Select "Public"

**Can't access URL?**
- Make sure you're signed into GitHub
- Check that port 5000 shows as "Running"

**Webhook not working?**
- Check Aspire Dashboard (port 15888) for logs
- Verify the URL includes `/api/webhook/azure`

## Stop/Restart

- **Stop Codespace**: Click "Codespaces" in bottom left → "Stop Current Codespace"
- **Restart**: Go to github.com/codespaces and click on your codespace
- **Delete**: Save money by deleting unused codespaces

## Tips

- Codespaces auto-stop after 30 minutes of inactivity
- URLs persist when you restart the same codespace
- Free tier includes 60 hours/month
- Use `gh codespace create` for CLI creation