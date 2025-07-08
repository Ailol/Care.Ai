# PowerShell script to test building the solution

Write-Host "Cleaning solution..." -ForegroundColor Green
dotnet clean

Write-Host "`nRestoring packages..." -ForegroundColor Green
dotnet restore

Write-Host "`nBuilding Care.Ai.Kernel.Abstractions..." -ForegroundColor Green
dotnet build packages/Care.Ai.Kernel.Abstractions/Care.Ai.Kernel.Abstractions.csproj

Write-Host "`nBuilding Care.Ai.Kernel..." -ForegroundColor Green
dotnet build packages/Care.Ai.Kernel/Care.Ai.Kernel.csproj

Write-Host "`nBuilding Care.Ai.Api..." -ForegroundColor Green
dotnet build src/Care.Ai.Api/Care.Ai.Api.csproj

Write-Host "`nBuilding solution..." -ForegroundColor Green
dotnet build Care.Ai.sln

Write-Host "`nBuild completed!" -ForegroundColor Green