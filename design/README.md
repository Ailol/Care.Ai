# Care.AI Workflow System

A minimalist workflow execution engine built on Dapr Actors and Semantic Kernel.

## Architecture

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   API       │────▶│   Dapr      │────▶│  Workflow   │
│ Controller  │     │   Actor     │     │   Actor     │
└─────────────┘     └─────────────┘     └─────────────┘
                           │
                           ▼
                    ┌─────────────┐
                    │  Semantic   │
                    │   Kernel    │
                    └─────────────┘
```

## Workflow Definition

```yaml
Name: PatientCare
Steps:
  - Id: assess
    Type: ai
    FunctionId: Medical.AssessSymptoms
    Parameters:
      symptoms: "{{symptoms}}"
    
  - Id: approve
    Type: human
    Assign: doctor@hospital.com
    When: assess == "urgent"
    
  - Id: notify
    Type: code
    FunctionId: System.SendNotification
```

## Step Types

- **ai**: Invokes a Semantic Kernel function with AI capabilities
- **human**: Waits for human approval (returns mock approval for now)
- **workflow**: Executes another workflow
- **code**: Executes a registered function

## API Endpoints

```bash
# Execute workflow from YAML
POST /api/workflow/execute-yaml
{
  "yaml": "...",
  "context": { "key": "value" }
}

# CRUD operations
POST   /api/workflow          # Save workflow
GET    /api/workflow/{id}     # Get workflow
DELETE /api/workflow/{id}     # Delete workflow

# Actions
POST /api/workflow/action/create    # Create workflow
POST /api/workflow/action/schedule  # Schedule workflow
```

## Running Tests

```bash
cd design
python3 test_api.py
```

## The Quantum Ogre

Yes, most step types just call `InvokeKernelFunction`. Sometimes that's exactly what you need - uniform simplicity over clever complexity.

## Running the System

```bash
# Start with Dapr
./run-dapr.sh

# Or with Aspire
dotnet run --project Care.Ai.AppHost
```

## Configuration

```json
{
  "Ollama": {
    "ModelId": "llama3.2",
    "Endpoint": "http://localhost:11434"
  }
}
```

## Design Philosophy

- **40 LOC limit**: If it doesn't fit, it splits
- **No defaults**: Missing config = throw
- **Quantum ogres**: Sometimes the illusion of choice is the right choice
- **Minimal abstractions**: 100 lines of clarity > 500 lines of cleverness

## Dependencies

- .NET 8.0
- Dapr 1.12+
- Semantic Kernel
- Ollama (for AI steps)
- Redis (for actor state)

## License

MIT