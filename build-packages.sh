#!/bin/bash

# Create local NuGet feed directory
mkdir -p packages/nupkg

# Build and pack Care.Ai.Kernel.Abstractions
echo "Building Care.Ai.Kernel.Abstractions..."
dotnet pack packages/Care.Ai.Kernel.Abstractions/Care.Ai.Kernel.Abstractions.csproj -c Release -o packages/nupkg

# Build and pack Care.Ai.Kernel
echo "Building Care.Ai.Kernel..."
dotnet pack packages/Care.Ai.Kernel/Care.Ai.Kernel.csproj -c Release -o packages/nupkg

echo "Packages built successfully in packages/nupkg/"