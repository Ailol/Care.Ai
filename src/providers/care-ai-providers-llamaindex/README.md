# LlamaIndex Provider for Care.AI

This is a Python-based LlamaIndex provider that runs as a Docker container and integrates with Care.AI.Kernel.

## Features

- Implements Care.AI.Kernel provider interface
- Uses LlamaIndex with Ollama backend
- Runs as a lightweight FastAPI service
- Auto-discovered by Care.AI.Kernel via Dapr

## Prerequisites

- Docker
- Ollama running on the host machine (port 11434)

## Build and Run

### Using Docker Compose
```bash
docker-compose up --build
```

### Using Docker directly
```bash
# Build
./build-docker.sh

# Run
docker run -p 5051:5051 -e OLLAMA_ENDPOINT=http://host.docker.internal:11434 care-ai-providers-llamaindex:latest
```

## Configuration

Environment variables:
- `OLLAMA_ENDPOINT`: Ollama server URL (default: http://ollama:11434)
- `OLLAMA_MODEL`: Model to use (default: qwen3-clean:latest)

## Endpoints

- `GET /manifest` - Provider manifest for discovery
- `POST /execute` - Execute AI operations
- `GET /health` - Health check

## Integration with Care.AI

The provider is automatically discovered by Care.AI.Kernel when running with Aspire.
It has a priority of 85 (lower than Semantic Kernel at 100).