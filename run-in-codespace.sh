#!/bin/bash

echo "🚀 Setting up Care.AI in Codespace..."

# Install .NET 9 if not available
if ! command -v dotnet &> /dev/null; then
    echo "📦 Installing .NET 9..."
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
    chmod +x dotnet-install.sh
    ./dotnet-install.sh --version 9.0.100 --install-dir /usr/local/dotnet
    export PATH="/usr/local/dotnet:$PATH"
    rm dotnet-install.sh
fi

# Build packages
echo "📦 Building Care.AI packages..."
if [ -f "./build-packages.sh" ]; then
    chmod +x ./build-packages.sh
    ./build-packages.sh
fi

# Display webhook URL
echo ""
echo "========================================="
echo "✅ Care.AI is ready!"
echo "========================================="
echo ""
echo "Your webhook URL is:"
echo "https://${CODESPACE_NAME}-5000.${GITHUB_CODESPACES_PORT_FORWARDING_DOMAIN}/api/webhook/azure"
echo ""
echo "To start the application, run:"
echo "dotnet run --project src/Care.Ai.AppHost"
echo ""