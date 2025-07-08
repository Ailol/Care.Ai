# Testing Care.Ai.Kernel API

## Prerequisites
Make sure the Care.Ai.Api service is running on port 51353 (or the appropriate port).

## Test Endpoints

### 1. Get Available Providers
```bash
curl -X GET http://localhost:51353/api/kernel/providers \
  -H "Content-Type: application/json"
```

Expected response: List of available AI providers with their capabilities.

### 2. Execute AI Request - Text Completion
```bash
curl -X POST http://localhost:51353/api/kernel/execute \
  -H "Content-Type: application/json" \
  -d '{
    "type": "text-completion",
    "data": {
      "prompt": "Hello, how are you?",
      "maxTokens": 100,
      "temperature": 0.7
    }
  }'
```

### 3. Execute AI Request with Specific Provider
```bash
curl -X POST http://localhost:51353/api/kernel/execute \
  -H "Content-Type: application/json" \
  -d '{
    "type": "text-completion",
    "data": {
      "provider": "semantic-kernel",
      "prompt": "Explain quantum computing in simple terms",
      "maxTokens": 200
    }
  }'
```

### 4. Execute with Smart Routing
```bash
curl -X POST http://localhost:51353/api/kernel/execute \
  -H "Content-Type: application/json" \
  -d '{
    "type": "text-completion",
    "data": {
      "useSmartRouting": true,
      "prompt": "What is the capital of France?",
      "maxTokens": 50
    }
  }'
```

### 5. Test Chat Completion
```bash
curl -X POST http://localhost:51353/api/kernel/execute \
  -H "Content-Type: application/json" \
  -d '{
    "type": "chat-completion",
    "data": {
      "messages": [
        {"role": "system", "content": "You are a helpful assistant."},
        {"role": "user", "content": "What is the weather like today?"}
      ],
      "maxTokens": 150
    }
  }'
```

### 6. Test Embedding Generation
```bash
curl -X POST http://localhost:51353/api/kernel/execute \
  -H "Content-Type: application/json" \
  -d '{
    "type": "embedding",
    "data": {
      "text": "This is a sample text for embedding generation",
      "model": "text-embedding-ada-002"
    }
  }'
```

### 7. Test with Invalid Request (Error Handling)
```bash
curl -X POST http://localhost:51353/api/kernel/execute \
  -H "Content-Type: application/json" \
  -d '{
    "type": "invalid-operation",
    "data": {}
  }'
```

## Using HTTPie (Alternative)
If you have HTTPie installed, here are equivalent commands:

```bash
# Get providers
http GET localhost:51353/api/kernel/providers

# Execute request
http POST localhost:51353/api/kernel/execute \
  type=text-completion \
  data:='{
    "prompt": "Hello, world!",
    "maxTokens": 100
  }'
```

## Testing with Dapr
If you want to test via Dapr sidecar:

```bash
# Invoke through Dapr
curl -X POST http://localhost:3500/v1.0/invoke/care-ai-api/method/api/kernel/execute \
  -H "Content-Type: application/json" \
  -d '{
    "type": "text-completion",
    "data": {
      "prompt": "Hello from Dapr!",
      "maxTokens": 100
    }
  }'
```

## Notes
- Replace `localhost:51353` with the actual host:port if different
- The actual response will depend on the AI providers configured
- Check application logs for detailed error messages if requests fail