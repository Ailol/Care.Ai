#!/bin/bash

echo "🚀 Setting up .NET 9 in Codespace..."

# Install required dependencies
echo "📦 Installing dependencies..."
apt-get update && apt-get install -y \
    wget \
    ca-certificates \
    libc6 \
    libgcc1 \
    libgssapi-krb5-2 \
    libicu70 \
    libssl3 \
    libstdc++6 \
    zlib1g

# Download and install .NET 9
echo "📦 Installing .NET 9..."
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0 --install-dir /usr/share/dotnet
ln -s /usr/share/dotnet/dotnet /usr/local/bin/dotnet
rm dotnet-install.sh

# Verify installation
echo "✅ Verifying installation..."
dotnet --version

# Install Aspire workload
echo "📦 Installing Aspire workload..."
dotnet workload install aspire

echo "✅ Setup complete!"