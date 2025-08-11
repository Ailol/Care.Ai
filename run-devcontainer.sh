#!/bin/bash

# Option 1: Using devcontainer CLI (if installed)
# npm install -g @devcontainers/cli
# devcontainer up --workspace-folder .
# devcontainer exec --workspace-folder . bash

# Option 2: Using Docker directly
echo "Building and running devcontainer..."

# Build the devcontainer image
docker build -t care-ai-devcontainer -f .devcontainer/Dockerfile .

# Run the container with all necessary mounts and settings
docker run -it --rm \
  --name care-ai-dev \
  -v "$(pwd):/workspace:cached" \
  -v "/var/run/docker.sock:/var/run/docker.sock" \
  -p 5001:5001 \
  -p 3500:3500 \
  -p 6050:6050 \
  -p 50005:50005 \
  -e REMOTE_CONTAINERS=true \
  --workdir /workspace \
  care-ai-devcontainer \
  bash -c "
    # Run post-create script
    bash /workspace/.devcontainer/post-create.sh
    
    # Start placement service in background
    nohup /home/vscode/start-placement.sh > /tmp/placement.log 2>&1 &
    
    # Give it a moment to start
    sleep 3
    
    echo 'DevContainer is ready!'
    echo 'Placement service is running on port 6050'
    echo 'To run the app: cd Care.Ai.AppHost && dotnet run'
    echo ''
    
    # Keep container running with bash
    exec bash
  "