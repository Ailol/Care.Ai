# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Care.AI.Kernel is a unified AI kernel gateway that routes requests to multiple AI orchestration platforms (Semantic Kernel, LlamaIndex, LangChain, etc.) via Dapr/MessageBus. It's a smart router, NOT an AI orchestrator itself.

## Key Architecture Principles

- **40 lines MAX per file** - NO exceptions
- **ONE class per file** - NEVER multiple classes
- **NO partial classes** - Use composition instead
- **Gateway Pattern**: Routes to providers, doesn't implement AI logic
- **Auto-Discovery**: Providers expose /manifest endpoint, gateway discovers automatically
- **Zero Maintenance**: Self-adaptive, no manual updates needed

## Build & Development

### Technology Stack

- .NET 9
- Aspire for orchestration
- Dapr for service mesh
- MessageBus for communication
- Directory.Packages.props for central package management

### Common Commands

```bash
# Build the solution
dotnet build

# Run with Aspire
dotnet run --project Care.AI.AppHost

# Run tests
dotnet test

# Run with Dapr locally
dapr run --app-id care-ai-kernel --app-port 5000 -- dotnet run
```

## MessageBus Patterns

### Setup

```csharp
builder.Services.AddMessageBus()
    .WithAllBuses("pubsub-rabbitmq")  // Matches Dapr component name
    .Build();
```

### Service Invocation

```csharp
var response = await _messageBus.Invoke.InvokeAsync<Response>(
    HttpMethod.Post,
    provider.ServiceId,      // Dapr app-id
    provider.ExecuteEndpoint, // e.g., "/execute"
    request);
```

### Dapr Component Required

Create `dapr/components/pubsub-rabbitmq.yaml`:

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: pubsub-rabbitmq
spec:
  type: pubsub.rabbitmq
  version: v1
  metadata:
    - name: host
      value: 'amqp://rabbitmq:5672'
```

## Project Structure

```
packages/
├── Care.AI.Kernel.Abstractions/
│   ├── IKernel.cs
│   ├── IRequest.cs
│   ├── IResponse.cs
│   ├── IKernelProviderManifest.cs
│   └── IProviderDiscovery.cs
│
└── Care.AI.Kernel/
    ├── Models/          # Request/Response DTOs
    ├── Configuration/   # Options classes
    ├── Discovery/       # Single KernelDiscovery.cs
    ├── Execution/       # Routing logic
    ├── Extensions/      # DI setup
    └── Internal/        # SmartRouting.cs only

src/
├── Care.AI.Api/         # API project (empty for now)
├── Care.AI.MCP/         # MCP project (empty for now)
└── Care.AI.AppHost/     # Aspire orchestration
```

## Core Execution Flow

All requests through ONE method - `ExecuteAsync()`:

1. Check for explicit provider → use it
2. Check for `useSmartRouting` → use AI to analyze
3. Check for model → find compatible provider
4. Default → use operation type to select provider

## Code Patterns

### Auto-Discovery

```csharp
services.Scan(scan => scan
    .FromCallingAssembly()
    .AddClasses(c => c.AssignableTo<IProviderDiscovery>())
    .AsImplementedInterfaces());
```

### Request Model

```csharp
public interface IRequest
{
    string Type { get; }             // "chat", "rag", "agent"
    Dictionary<string, object> Data { get; }
    Dictionary<string, object>? Metadata { get; }
}
```

### Provider Manifest

```csharp
public interface IKernelProviderManifest
{
    string ProviderId { get; }
    List<string> AvailableModels { get; }
    List<string> Operations { get; }
    string ServiceId { get; }  // Dapr app-id
    string ExecuteEndpoint { get; }
    int Priority { get; }
}
```

## Important Notes

1. **MessageBus First**: Always use MessageBus for provider communication
2. **CloudEvents**: Events are automatically wrapped in CloudEvent format
3. **Resilience**: Built-in retry, circuit breaker, timeout handling
4. **Telemetry**: Automatic Activity tracking for observability
5. **No Hardcoding**: Everything discovered at runtime via manifests

## Testing Strategy

dont add these yet

- Unit tests for each component
- Integration tests with Dapr test containers
- Use xUnit with async test patterns
- Mock MessageBus for isolated testing

## Common Pitfalls to Avoid

1. Don't create files over 40 lines
2. Don't hardcode provider information
3. Don't bypass MessageBus for communication
4. Don't implement AI logic in the gateway
5. Don't use multiple classes per file
