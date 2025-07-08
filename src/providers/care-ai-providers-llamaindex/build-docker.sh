#!/bin/bash
# Build the LlamaIndex provider Docker image

echo "Building LlamaIndex provider Docker image..."
docker build -t care-ai-providers-llamaindex:latest .

echo "Done! You can now run the container with:"
echo "docker run -p 5051:5051 -e OLLAMA_ENDPOINT=http://host.docker.internal:11434 care-ai-providers-llamaindex:latest"