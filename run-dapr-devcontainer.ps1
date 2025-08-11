# Run devcontainer without VS Code using the devcontainer CLI
Write-Host "Starting Dapr services using devcontainer..." -ForegroundColor Green

# Install devcontainer CLI if not present
if (!(Get-Command devcontainer -ErrorAction SilentlyContinue)) {
    Write-Host "Installing devcontainer CLI..." -ForegroundColor Yellow
    npm install -g @devcontainers/cli
}

# Stop any existing container
docker stop dapr-services 2>$null
docker rm dapr-services 2>$null

# Start the devcontainer
Write-Host "Starting devcontainer with Dapr services..." -ForegroundColor Green
devcontainer up --workspace-folder . --config .devcontainer/devcontainer.json

Write-Host "`nDapr services are running:" -ForegroundColor Green
Write-Host "  - Placement: localhost:6050" -ForegroundColor Cyan
Write-Host "  - Scheduler: localhost:6060" -ForegroundColor Cyan

# Show container status
docker ps --filter name=dapr-services --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"