# Install Dapr CLI on Windows
Write-Host "Installing Dapr CLI..." -ForegroundColor Green

# Check if Dapr is already installed
$daprPath = "$env:USERPROFILE\.dapr\bin\dapr.exe"
if (Test-Path $daprPath) {
    Write-Host "Dapr CLI is already installed" -ForegroundColor Yellow
    & $daprPath version
} else {
    # Download and install Dapr CLI
    $installScript = Invoke-WebRequest -Uri "https://raw.githubusercontent.com/dapr/cli/master/install/install.ps1" -UseBasicParsing
    $installScript.Content | Invoke-Expression
}

# Initialize Dapr in standalone mode
Write-Host "`nInitializing Dapr..." -ForegroundColor Green
& "$env:USERPROFILE\.dapr\bin\dapr.exe" init

Write-Host "`nDapr installation complete!" -ForegroundColor Green
Write-Host "Placement service should now be running on localhost:6050" -ForegroundColor Cyan
Write-Host "You can now run your Aspire app from Visual Studio" -ForegroundColor Yellow