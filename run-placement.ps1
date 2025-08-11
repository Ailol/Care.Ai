# Script to run Dapr placement and scheduler services
Write-Host "Starting Dapr services (placement & scheduler)..." -ForegroundColor Green

cd dapr-placement
docker-compose up -d

Write-Host "Dapr services are running:" -ForegroundColor Green
Write-Host "  - Placement service on port 6050" -ForegroundColor Cyan
Write-Host "  - Scheduler service on port 6060" -ForegroundColor Cyan
Write-Host "You can now run your Aspire app from Visual Studio" -ForegroundColor Yellow