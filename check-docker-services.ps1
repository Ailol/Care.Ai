# Check if Docker services are running
Write-Host "Checking Docker containers..." -ForegroundColor Yellow

# Check all running containers
Write-Host "`nRunning containers:" -ForegroundColor Cyan
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

# Check if dapr-services container exists
$daprContainer = docker ps -a -q -f name=dapr-services
if ($daprContainer) {
    Write-Host "`nDapr services container found. Checking logs:" -ForegroundColor Green
    docker logs dapr-services --tail 30
} else {
    Write-Host "`nDapr services container not found!" -ForegroundColor Red
    Write-Host "Run: .\run-placement.ps1" -ForegroundColor Yellow
}

# Test connectivity directly
Write-Host "`nTesting port connectivity:" -ForegroundColor Cyan
Test-NetConnection -ComputerName localhost -Port 6050 -InformationLevel Detailed
Test-NetConnection -ComputerName localhost -Port 6060 -InformationLevel Detailed