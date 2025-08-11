# Script to run Dapr placement and scheduler services in a container
Write-Host "Building Dapr services container..." -ForegroundColor Green
docker build -t dapr-services -f .devcontainer/Dockerfile .devcontainer

Write-Host "Running Dapr services container..." -ForegroundColor Green

# Stop any existing container
docker stop dapr-services 2>$null
docker rm dapr-services 2>$null

# Run the container
docker run -d `
  --name dapr-services `
  -p 6050:50005 `
  -p 6060:50006 `
  dapr-services

Write-Host "Dapr services started:" -ForegroundColor Green
Write-Host "  - Placement service on port 6050" -ForegroundColor Cyan
Write-Host "  - Scheduler service on port 6060" -ForegroundColor Cyan
Write-Host "`nYou can now run your Aspire app from Visual Studio" -ForegroundColor Yellow

# Check if services are accessible
Start-Sleep -Seconds 2
Write-Host "`nChecking services..." -ForegroundColor Yellow
$placement = Test-NetConnection -ComputerName localhost -Port 6050 -InformationLevel Quiet
$scheduler = Test-NetConnection -ComputerName localhost -Port 6060 -InformationLevel Quiet

if ($placement) {
    Write-Host "✓ Placement service is accessible" -ForegroundColor Green
} else {
    Write-Host "✗ Placement service is NOT accessible" -ForegroundColor Red
}

if ($scheduler) {
    Write-Host "✓ Scheduler service is accessible" -ForegroundColor Green
} else {
    Write-Host "✗ Scheduler service is NOT accessible" -ForegroundColor Red
}

# Show container logs
Write-Host "`nContainer logs:" -ForegroundColor Yellow
docker logs dapr-services --tail 20