#!/bin/bash

echo "Starting Dapr placement service on port 50005..."
/dapr/bin/placement -port 50005 -log-level debug &
PLACEMENT_PID=$!

echo "Starting Dapr scheduler service on port 50006..."
/dapr/bin/scheduler -port 50006 -log-level debug &
SCHEDULER_PID=$!

# Wait a bit for services to start
sleep 2

# Check if services are running
if ps -p $PLACEMENT_PID > /dev/null; then
    echo "✓ Placement service started (PID: $PLACEMENT_PID)"
else
    echo "✗ Placement service failed to start"
fi

if ps -p $SCHEDULER_PID > /dev/null; then
    echo "✓ Scheduler service started (PID: $SCHEDULER_PID)"
else
    echo "✗ Scheduler service failed to start"
fi

echo ""
echo "Dapr services listening on:"
echo "  - Placement: 0.0.0.0:50005 (mapped to host port 6050)"
echo "  - Scheduler: 0.0.0.0:50006 (mapped to host port 6060)"
echo ""
echo "Tailing logs..."

# Keep container running and show logs
tail -f /dev/null