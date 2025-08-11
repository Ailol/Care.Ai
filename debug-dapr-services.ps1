# Debug script to check Dapr services
Write-Host "Checking Dapr services container..." -ForegroundColor Yellow

# Check if container is running
$container = docker ps -q -f name=dapr-services
if ($container) {
    Write-Host "Container is running. Getting logs:" -ForegroundColor Green
    docker logs dapr-services
    
    Write-Host "`nChecking processes inside container:" -ForegroundColor Yellow
    docker exec dapr-services ps aux
    
    Write-Host "`nChecking network connectivity inside container:" -ForegroundColor Yellow
    docker exec dapr-services sh -c "netstat -tlnp 2>/dev/null || ss -tlnp"
} else {
    Write-Host "Container is not running!" -ForegroundColor Red
    
    # Check if container exists but stopped
    $stoppedContainer = docker ps -a -q -f name=dapr-services
    if ($stoppedContainer) {
        Write-Host "Container exists but is stopped. Logs:" -ForegroundColor Yellow
        docker logs dapr-services --tail 50
    }
}

# Try to connect directly with curl to see what's listening
Write-Host "`nTrying direct connection to placement port:" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:6050" -Method Get -ErrorAction Stop
    Write-Host "Got response: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "Connection failed: $_" -ForegroundColor Red
}

# Check with netcat if available
Write-Host "`nPort scan results:" -ForegroundColor Yellow
Test-NetConnection -ComputerName localhost -Port 6050 | Select-Object ComputerName, RemotePort, TcpTestSucceeded
Test-NetConnection -ComputerName localhost -Port 6060 | Select-Object ComputerName, RemotePort, TcpTestSucceeded