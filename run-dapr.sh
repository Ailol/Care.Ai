#!/bin/bash

# Start Dapr placement service in background
echo "Starting Dapr placement service..."
dapr run --app-id placement --app-port 50005 --dapr-http-port 3501 -- placement -port 50005 &
PLACEMENT_PID=$!

# Wait a bit for placement to start
sleep 3

# Start the API with Dapr
echo "Starting Care.AI API with Dapr..."
cd src/Care.AI.Api
dapr run --app-id care-ai-api --app-port 5001 --dapr-http-port 3500 --dapr-grpc-port 50001 --components-path ../../Care.Ai.AppHost/components -- dotnet run

# Clean up placement when done
kill $PLACEMENT_PID