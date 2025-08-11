# Download and run Dapr placement and scheduler natively on Windows

$daprVersion = "1.15.6"
$daprPath = "$env:USERPROFILE\.dapr\bin"

# Create directory if it doesn't exist
if (!(Test-Path $daprPath)) {
    New-Item -ItemType Directory -Path $daprPath -Force
}

# Download placement if not exists
$placementPath = "$daprPath\placement.exe"
if (!(Test-Path $placementPath)) {
    Write-Host "Downloading Dapr placement..." -ForegroundColor Yellow
    $placementUrl = "https://github.com/dapr/dapr/releases/download/v$daprVersion/placement_windows_amd64.zip"
    $placementZip = "$daprPath\placement.zip"
    Invoke-WebRequest -Uri $placementUrl -OutFile $placementZip
    Expand-Archive -Path $placementZip -DestinationPath $daprPath -Force
    Remove-Item $placementZip
}

# Download scheduler if not exists
$schedulerPath = "$daprPath\scheduler.exe"
if (!(Test-Path $schedulerPath)) {
    Write-Host "Downloading Dapr scheduler..." -ForegroundColor Yellow
    $schedulerUrl = "https://github.com/dapr/dapr/releases/download/v$daprVersion/scheduler_windows_amd64.zip"
    $schedulerZip = "$daprPath\scheduler.zip"
    Invoke-WebRequest -Uri $schedulerUrl -OutFile $schedulerZip
    Expand-Archive -Path $schedulerZip -DestinationPath $daprPath -Force
    Remove-Item $schedulerZip
}

# Kill any existing processes
Write-Host "Stopping any existing Dapr services..." -ForegroundColor Yellow
Get-Process -Name "placement" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process -Name "scheduler" -ErrorAction SilentlyContinue | Stop-Process -Force

# Start placement
Write-Host "Starting Dapr placement service on port 6050..." -ForegroundColor Green
Start-Process -FilePath $placementPath -ArgumentList "-port", "50005" -WindowStyle Hidden

# Start scheduler
Write-Host "Starting Dapr scheduler service on port 6060..." -ForegroundColor Green
Start-Process -FilePath $schedulerPath -ArgumentList "-port", "50006" -WindowStyle Hidden

Start-Sleep -Seconds 2

# Check if services are running
if (Get-Process -Name "placement" -ErrorAction SilentlyContinue) {
    Write-Host "✓ Placement service is running" -ForegroundColor Green
} else {
    Write-Host "✗ Placement service failed to start" -ForegroundColor Red
}

if (Get-Process -Name "scheduler" -ErrorAction SilentlyContinue) {
    Write-Host "✓ Scheduler service is running" -ForegroundColor Green
} else {
    Write-Host "✗ Scheduler service failed to start" -ForegroundColor Red
}

Write-Host "`nDapr services should now be available on:" -ForegroundColor Cyan
Write-Host "  - Placement: localhost:6050" -ForegroundColor White
Write-Host "  - Scheduler: localhost:6060" -ForegroundColor White
Write-Host "`nYou can now run your Aspire app from Visual Studio" -ForegroundColor Yellow