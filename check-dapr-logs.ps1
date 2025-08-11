# Check Docker container status and logs
Write-Host "Checking Dapr services container..." -ForegroundColor Yellow

# List all containers (including stopped ones)
Write-Host "`nAll containers:" -ForegroundColor Cyan
docker ps -a --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

# Check logs from the dapr-services container
Write-Host "`nDapr services container logs:" -ForegroundColor Cyan
docker logs dapr-services --tail 20

# Check if container is running
$container = docker ps -q -f name=dapr-services
if ($container) {
    Write-Host "`nContainer is running. Checking processes inside:" -ForegroundColor Green
    docker exec dapr-services ps aux
} else {
    Write-Host "`nContainer is not running!" -ForegroundColor Red
}