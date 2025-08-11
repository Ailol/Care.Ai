# Care.AI API

A test API for the Care.AI package demonstrating Dapr actors with Semantic Kernel integration.

## Running the API

### Prerequisites
- .NET 8.0 SDK
- Dapr CLI installed
- Ollama running locally (or Azure OpenAI configured)

### With Dapr (recommended)
```bash
dapr run --app-id care-ai-api --app-port 5200 --dapr-http-port 3500 --dapr-grpc-port 50001 -- dotnet run
```

### Without Dapr (limited functionality)
```bash
dotnet run
```

## Endpoints

### Chat
- `POST /chat` - Send a chat message
  ```json
  {
    "userId": "user123",
    "message": "Hello, how are you?",
    "sessionId": "session456"
  }
  ```

### Chat History
- `GET /chat/history/{userId}/{sessionId}` - Get chat history for a session

### Execute Kernel Function
- `POST /execute` - Execute a Semantic Kernel function
  ```json
  {
    "userId": "user123",
    "function": "LangChain.Chain",
    "args": {
      "prompt": "Explain quantum computing"
    }
  }
  ```

## Configuration

Update `appsettings.json` to configure:

### Ollama (default)
```json
{
  "Ollama": {
    "ModelId": "qwen3-clean:latest",
    "Endpoint": "http://localhost:11434"
  }
}
```

### Azure OpenAI (optional)
```json
{
  "AzureOpenAI": {
    "DeploymentName": "gpt-35-turbo",
    "Endpoint": "https://your-endpoint.openai.azure.com/",
    "ApiKey": "your-api-key"
  }
}
```

## Swagger UI

When running in development mode, access Swagger UI at:
- http://localhost:5200/swagger