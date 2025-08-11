#!/bin/bash
echo "Checking Dapr services in devcontainer..."

# Check if placement is running
if pgrep -x "placement" > /dev/null; then
    echo "✓ Placement service is running"
else
    echo "✗ Placement service is NOT running"
fi

# Check if scheduler is running
if pgrep -x "scheduler" > /dev/null; then
    echo "✓ Scheduler service is running"
else
    echo "✗ Scheduler service is NOT running"
fi

# Check listening ports
echo -e "\nListening ports:"
netstat -tlnp 2>/dev/null | grep -E "6050|6060|50005|50006" || ss -tlnp | grep -E "6050|6060|50005|50006"

# Try to start services if not running
if ! pgrep -x "placement" > /dev/null; then
    echo -e "\nStarting placement service..."
    nohup dapr run --app-id placement --app-port 50005 --dapr-grpc-port 6050 -- placement > /tmp/placement.log 2>&1 &
    sleep 2
fi

if ! pgrep -x "scheduler" > /dev/null; then
    echo -e "\nStarting scheduler service..."
    nohup dapr run --app-id scheduler --app-port 50006 --dapr-grpc-port 6060 -- scheduler > /tmp/scheduler.log 2>&1 &
    sleep 2
fi

echo -e "\nServices status after startup attempt:"
ps aux | grep -E "placement|scheduler" | grep -v grep