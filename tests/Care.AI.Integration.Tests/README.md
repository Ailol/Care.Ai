# Care.AI Integration Tests

## Overview
Comprehensive integration tests for the Care.AI platform covering all current and planned capabilities.

## Running the Tests

### Prerequisites
- .NET 9 SDK
- Docker (for external services)
- Ollama (optional, for LLM tests)

### Quick Start
```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test category
dotnet test --filter "Category=Current"
```

### Test Categories

#### Current Implementation Tests (Should Pass)
- `Test01_KernelDiscovery_ShouldFindAllProviders` ✅
- `Test02_SemanticKernel_SimpleChatCompletion` ✅ (if Ollama running)
- `Test05_ProviderManifest_Serialization` ✅
- `Test06_MultipleFrameworkRegistration` ✅
- `Test07_RequestResponse_Contract` ✅

#### Not Yet Implemented (Expected to Fail)
- `Test03_SmartRouting_ShouldRouteToCorrectProvider` ❌
- `Test04_LlamaIndex_EmbeddingGeneration` ❌
- `Test09_LlamaIndex_NotImplementedOperations` ❌

#### Future Features (Skipped)
- Dapr Workflow tests
- Event-driven tests
- Visual pipeline tests

## Test Environment Setup

### Option 1: Full Environment
```bash
# Start all services
docker-compose up -d

# Includes:
# - PostgreSQL with pgvector
# - Neo4j
# - Redis
# - RabbitMQ
# - Ollama
```

### Option 2: Minimal Testing
```bash
# Just run tests without external dependencies
# Only framework discovery and manifest tests will pass
dotnet test --filter "Category!=Integration"
```

## Understanding Test Results

### Green Tests ✅
These validate the current working implementation:
- Framework discovery
- Provider manifests
- Basic request routing
- Configuration loading

### Red Tests ❌
These document what needs to be implemented:
- Vector store operations
- RAG functionality
- Workflow orchestration
- Event handling

### Yellow Tests ⚠️
These depend on external services:
- Ollama for LLM operations
- PostgreSQL for vector storage
- Dapr for workflows

## Adding New Tests

When adding new capabilities, follow this pattern:

```csharp
[Fact]
public async Task Test_NewFeature_ShouldWork()
{
    // Arrange
    var request = new Request { /* ... */ };
    
    // Act
    var response = await _kernel.ExecuteAsync(request);
    
    // Assert
    Assert.NotNull(response);
    Assert.True(response.Success);
}
```

## Test-Driven Development Flow

1. Write test for new feature (it will fail)
2. Implement the feature
3. Test passes
4. Refactor if needed
5. All tests still pass

## Coverage Goals

- Unit Tests: 80% code coverage
- Integration Tests: All major workflows
- E2E Tests: Critical user journeys
- Performance Tests: Sub-second response times