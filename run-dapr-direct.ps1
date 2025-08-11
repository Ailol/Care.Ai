# Run Dapr services directly with Docker
Write-Host "Starting Dapr services..." -ForegroundColor Green

# Stop any existing container
docker stop dapr-services 2>$null
docker rm dapr-services 2>$null

# Run the container
docker run -d `
  --name dapr-services `
  -p 6050:50005 `
  -p 6060:50006 `
  daprio/dapr:1.15.6 `
  sh -c "placement -port 50005 & scheduler -port 50006 & tail -f /dev/null"

Write-Host "Dapr services started:" -ForegroundColor Green
Write-Host "  - Placement: localhost:6050" -ForegroundColor Cyan  
Write-Host "  - Scheduler: localhost:6060" -ForegroundColor Cyan

# Wait and check
Start-Sleep -Seconds 2
docker ps --filter name=dapr-services --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"