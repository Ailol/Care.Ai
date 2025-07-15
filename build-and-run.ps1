#!/usr/bin/env pwsh
# Build and Run script for Care.AI

param(
    [switch]$SkipPackageBuild,
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

# Clean if requested
if ($Clean) {
    Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
    Remove-Item -Path "packages/nupkg/*.nupkg" -Force -ErrorAction SilentlyContinue
    dotnet clean
}

# Build NuGet packages
if (-not $SkipPackageBuild) {
    Write-Host "Building NuGet packages..." -ForegroundColor Green
    
    # Build Care.Ai.Kernel.Abstractions
    Write-Host "  - Building Care.Ai.Kernel.Abstractions..." -ForegroundColor Cyan
    dotnet pack packages/Care.Ai.Kernel.Abstractions/Care.Ai.Kernel.Abstractions.csproj `
        -c Release `
        -o packages/nupkg `
        --no-restore
    
    # Build Care.Ai.Kernel
    Write-Host "  - Building Care.Ai.Kernel..." -ForegroundColor Cyan
    dotnet pack packages/Care.Ai.Kernel/Care.Ai.Kernel.csproj `
        -c Release `
        -o packages/nupkg `
        --no-restore
    
    # Build Care.Ai.Providers.SemanticKernel
    Write-Host "  - Building Care.Ai.Providers.SemanticKernel..." -ForegroundColor Cyan
    dotnet pack packages/Care.Ai.Providers.SemanticKernel/Care.Ai.Providers.SemanticKernel.csproj `
        -c Release `
        -o packages/nupkg `
        --no-restore
    
    Write-Host "NuGet packages built successfully!" -ForegroundColor Green
}

# Clear NuGet cache for our packages to ensure fresh versions
Write-Host "Clearing NuGet cache for Care.AI packages..." -ForegroundColor Yellow
dotnet nuget locals all --clear

# Run the AppHost
Write-Host "Starting Care.AI AppHost..." -ForegroundColor Green
dotnet run --project Care.Ai.AppHost/Care.Ai.AppHost.csproj