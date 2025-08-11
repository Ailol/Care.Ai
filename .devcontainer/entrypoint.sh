#!/bin/bash

# Start placement service
echo "Starting Dapr placement service..."
/home/vscode/.dapr/bin/placement -port 50005 &

# Start scheduler service
echo "Starting Dapr scheduler service..."
/home/vscode/.dapr/bin/scheduler -port 50006 &

# Keep container running
echo "Dapr services started. Placement on 6050, Scheduler on 6060"
exec "$@"