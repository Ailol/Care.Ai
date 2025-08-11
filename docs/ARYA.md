# Arya - Conversational AI Service

Arya is an intelligent conversational AI service that bridges natural language queries with structured data operations and workflow creation.

## How It Works

### 1. Natural Language Processing
When a user asks a question like "I want to check which agreements will expire within a month?", Arya:
- Analyzes the intent using Semantic Kernel
- Automatically identifies and calls relevant kernel functions
- Returns structured data in JSON format

### 2. Function Calling Flow
```
User Query → Arya → Semantic Kernel → Kernel Functions → Structured Data
```

Example:
- User: "Check expiring agreements"
- Arya calls: `Agreement.GetExpiringAgreements(withinDays: 30)`
- Returns: JSON array of agreement objects

### 3. Response Structure
```json
{
  "message": "I found 4 agreements expiring within 30 days",
  "data": [
    {
      "id": "AGR-001",
      "name": "Service Agreement - Clinic A",
      "expiryDate": "2024-01-15",
      "clinicId": "CLINIC-001",
      "status": "Active"
    }
  ],
  "actions": ["CreateWorkflow", "ExportData", "ScheduleReport"],
  "context": {
    "sessionId": "abc123",
    "lastQueryData": {...}
  }
}
```

### 4. Follow-up Actions
After receiving data, Arya suggests contextual actions:
- **CreateWorkflow**: Generate automated workflows based on the query
- **ExportData**: Export results in various formats
- **ScheduleReport**: Set up recurring reports
- **ShareResults**: Share data with team members

### 5. Workflow Creation
When user selects "CreateWorkflow", Arya:
- Uses the context from the previous query
- Generates a workflow definition
- Configures appropriate triggers and schedules

## API Endpoints

### Chat Endpoint
```
POST /api/arya/chat
{
  "message": "Your question here",
  "sessionId": "optional-session-id",
  "context": { "optional": "context" }
}
```

### Action Endpoint
```
POST /api/arya/action/{action}
{
  "context": { "from": "previous response" },
  "workflowName": "Optional custom name",
  "format": "json|csv|excel",
  "schedule": "daily|weekly|monthly"
}
```

## Example Conversation Flow

1. **User**: "Which agreements expire soon?"
2. **Arya**: Returns agreements + suggests actions
3. **User**: Clicks "Create Workflow"
4. **Arya**: Creates automated monitoring workflow
5. **Result**: Weekly emails about expiring agreements

## Benefits

- **No Frontend Required**: Works entirely through API
- **Context-Aware**: Maintains conversation context
- **Action-Oriented**: Suggests next steps
- **Workflow Integration**: Seamlessly creates automation
- **Extensible**: Easy to add new kernel functions