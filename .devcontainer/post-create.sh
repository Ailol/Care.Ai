#!/bin/bash

# Install Dapr CLI
wget -q https://raw.githubusercontent.com/dapr/cli/master/install/install.sh -O - | /bin/bash

# Initialize Dapr in slim mode (no containers)
dapr init -s

# Start placement service in background
echo "Starting placement service..."
nohup /home/vscode/.dapr/bin/placement -port 50005 > /tmp/placement.log 2>&1 &

# Start scheduler service in background  
echo "Starting scheduler service..."
nohup /home/vscode/.dapr/bin/scheduler -port 50006 > /tmp/scheduler.log 2>&1 &

# Give services time to start
sleep 3

# Install .NET Aspire workload
dotnet workload install aspire

# Restore all projects
cd /workspace && dotnet restore || true

# Create a systemd-like service script for placement
cat > /home/vscode/start-placement.sh << 'EOF'
#!/bin/bash
echo "Starting Dapr placement service..."
dapr run --app-id placement --app-port 50005 --dapr-grpc-port 6050 -- placement
EOF

chmod +x /home/vscode/start-placement.sh

# Create a supervisor config to run placement automatically
sudo mkdir -p /etc/supervisor/conf.d
sudo tee /etc/supervisor/conf.d/dapr-placement.conf > /dev/null << EOF
[program:dapr-placement]
command=/home/vscode/start-placement.sh
autostart=true
autorestart=true
stderr_logfile=/var/log/dapr-placement.err.log
stdout_logfile=/var/log/dapr-placement.out.log
user=vscode
environment=HOME="/home/vscode",USER="vscode"
EOF

# Start supervisor
sudo service supervisor start || true

echo "Development container setup complete!"
echo "Dapr placement service is running automatically on port 6050"