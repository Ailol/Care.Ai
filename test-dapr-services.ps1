# Test if Dapr services are accessible
Write-Host "Testing Dapr services connectivity..." -ForegroundColor Yellow

# Test placement service
try {
    $placement = Test-NetConnection -ComputerName localhost -Port 6050 -InformationLevel Quiet
    if ($placement) {
        Write-Host "✓ Placement service is accessible on port 6050" -ForegroundColor Green
    } else {
        Write-Host "✗ Placement service is NOT accessible on port 6050" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Error testing placement service: $_" -ForegroundColor Red
}

# Test scheduler service  
try {
    $scheduler = Test-NetConnection -ComputerName localhost -Port 6060 -InformationLevel Quiet
    if ($scheduler) {
        Write-Host "✓ Scheduler service is accessible on port 6060" -ForegroundColor Green
    } else {
        Write-Host "✗ Scheduler service is NOT accessible on port 6060" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Error testing scheduler service: $_" -ForegroundColor Red
}

# Check Docker containers
Write-Host "`nDocker containers:" -ForegroundColor Yellow
docker ps | Select-String "dapr"