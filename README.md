# Care.Ai.Kernel

A unified AI kernel gateway that routes requests to multiple AI orchestration platforms via Dapr/MessageBus.

## Quick Start

### Prerequisites
- .NET 9 SDK
- Docker (for Dapr and dependencies)
- Dapr CLI

### Building the Project

1. Build NuGet packages:
```bash
./build-packages.sh
```

2. Build the solution:
```bash
dotnet build
```

### Running with Aspire

```bash
dotnet run --project src/Care.Ai.AppHost
```

### Running with Dapr

```bash
# Start Care.Ai.Api with Dapr
dapr run --app-id care-ai-api --app-port 5000 --dapr-http-port 3500 -- dotnet run --project src/Care.Ai.Api
```

## Architecture

Care.Ai.Kernel acts as a smart router that:
- Auto-discovers AI providers via manifest endpoints
- Routes requests based on model availability
- Provides optional AI-powered routing
- Maintains zero coupling with provider internals

## API Usage

### Execute AI Request
```http
POST /api/kernel/execute
Content-Type: application/json

{
  "type": "chat",
  "data": {
    "model": "gpt-4",
    "message": "Hello, world!"
  }
}
```

### Get Available Providers
```http
GET /api/kernel/providers
```

## Configuration

Edit `appsettings.json` to configure providers and routing:

```json
{
  "CareAi": {
    "Kernel": {
      "SmartRoutingProvider": "semantic-kernel",
      "SmartRoutingModel": "gpt-3.5-turbo",
      "KnownProviders": ["semantic-kernel", "llamaindex"]
    }
  }
}
```