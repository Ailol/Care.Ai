# Build and run devcontainer without VS Code

Write-Host "Building devcontainer image..." -ForegroundColor Green
docker build -t care-ai-devcontainer -f .devcontainer/Dockerfile .

Write-Host "Running devcontainer..." -ForegroundColor Green

# Run the container
docker run -it --rm `
  --name care-ai-dev `
  -v "${PWD}:/workspace:cached" `
  -v "/var/run/docker.sock:/var/run/docker.sock" `
  -p 5001:5001 `
  -p 3500:3500 `
  -p 6050:50005 `
  -p 6060:50006 `
  -e REMOTE_CONTAINERS=true `
  --workdir /workspace `
  care-ai-devcontainer `
  bash -c "/home/vscode/.dapr/bin/placement -port 50005 & /home/vscode/.dapr/bin/scheduler -port 50006 & echo 'Dapr services started - Placement on 6050, Scheduler on 6060' && exec bash"