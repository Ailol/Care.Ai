# RAG Provider Design

## Overview

A specialized RAG (Retrieval-Augmented Generation) provider that integrates with the Care.AI.Kernel gateway. This provider handles document indexing, vector search, and context-aware retrieval for RAG operations, with support for multiple embedding models and storage backends.

## Architecture

### Provider Structure
```
packages/Care.Ai.Frameworks.Rag/
├── Care.Ai.Frameworks.Rag.csproj
├── RagFramework.cs              # IProviderDiscovery implementation
├── Models/
│   ├── RagRequest.cs            # RAG-specific request model
│   └── RagResponse.cs           # RAG-specific response model
├── Services/
│   ├── RagProcessor.cs          # Core RAG operations
│   ├── VectorStoreService.cs    # Document storage and retrieval
│   └── EmbeddingService.cs      # Embedding generation
└── ServiceCollectionExtensions.cs # DI registration
```

## Core Components

### 1. Provider Manifest
```csharp
public class RagManifest : IKernelProviderManifest
{
    public string ProviderId => "rag-provider";
    public List<string> AvailableModels => ["nomic-embed-text", "all-minilm"];
    public List<string> Operations => ["rag", "index", "search"];
    public string ServiceId => "rag-service";
    public string ExecuteEndpoint => "in-process";
    public int Priority => 90;
}
```

### 2. RAG Request Model
```csharp
public class RagRequest : IRequest
{
    public string Type => "rag";
    public Dictionary<string, object> Data { get; set; } = new();
    public Dictionary<string, object>? Metadata { get; set; }
    
    // RAG-specific properties
    public string Query { get; set; } = string.Empty;
    public string? IndexId { get; set; }
    public string? IndexPath { get; set; }  // Can be local path or URL
    public int TopK { get; set; } = 5;
    public string? Model { get; set; }
    public RagMode Mode { get; set; } = RagMode.Query;
}

public enum RagMode
{
    Query,      // Standard RAG query
    Index,      // Document indexing
    Search,     // Vector search only
    Generate    // Generation with context
}
```

### 3. RAG Response Model
```csharp
public class RagResponse : IResponse
{
    public bool Success { get; set; }
    public string Type => "rag-response";
    public object? Result { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    
    // RAG-specific properties
    public string? GeneratedText { get; set; }
    public List<RetrievedDocument> RetrievedDocuments { get; set; } = new();
    public float? ConfidenceScore { get; set; }
    public string? IndexId { get; set; }
}

public class RetrievedDocument
{
    public string Content { get; set; } = string.Empty;
    public float Score { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
```

### 4. Core Services

#### RagService.cs (40 lines max)
```csharp
public class RagService
{
    private readonly ILogger<RagService> _logger;
    private readonly LlamaIndexOptions _options;
    
    public async Task<RagResponse> ProcessRagRequestAsync(
        RagRequest request, 
        CancellationToken cancellationToken)
    {
        return request.Mode switch
        {
            RagMode.Query => await QueryWithRagAsync(request, cancellationToken),
            RagMode.Index => await IndexDocumentsAsync(request, cancellationToken),
            RagMode.Search => await SearchOnlyAsync(request, cancellationToken),
            RagMode.Generate => await GenerateWithContextAsync(request, cancellationToken),
            _ => throw new ArgumentException($"Unsupported RAG mode: {request.Mode}")
        };
    }
    
    private async Task<RagResponse> QueryWithRagAsync(RagRequest request, CancellationToken ct)
    {
        // Retrieve relevant documents
        var documents = await RetrieveDocumentsAsync(request.Query, request.IndexId, request.TopK, ct);
        
        // Generate response with context
        var context = string.Join("\n\n", documents.Select(d => d.Content));
        var prompt = BuildRagPrompt(request.Query, context);
        var response = await GenerateResponseAsync(prompt, request.Model, ct);
        
        return new RagResponse
        {
            Success = true,
            GeneratedText = response,
            RetrievedDocuments = documents,
            IndexId = request.IndexId
        };
    }
}
```

#### IndexService.cs (40 lines max)
```csharp
public class IndexService
{
    private readonly ILogger<IndexService> _logger;
    private readonly string _indexPath;
    
    public async Task<string> CreateIndexAsync(
        IEnumerable<DocumentRequest> documents,
        string? indexId = null,
        CancellationToken cancellationToken = default)
    {
        indexId ??= Guid.NewGuid().ToString();
        var indexDirectory = Path.Combine(_indexPath, indexId);
        Directory.CreateDirectory(indexDirectory);
        
        // Process documents into vector embeddings
        var embeddings = await ProcessDocumentsAsync(documents, cancellationToken);
        
        // Store in vector database (ChromaDB, Pinecone, etc.)
        await StoreEmbeddingsAsync(indexDirectory, embeddings, cancellationToken);
        
        _logger.LogInformation("Created index {IndexId} with {Count} documents", 
            indexId, documents.Count());
            
        return indexId;
    }
    
    private async Task<List<DocumentEmbedding>> ProcessDocumentsAsync(
        IEnumerable<DocumentRequest> documents, 
        CancellationToken cancellationToken)
    {
        var embeddings = new List<DocumentEmbedding>();
        
        foreach (var doc in documents)
        {
            var chunks = ChunkDocument(doc.Content);
            var docEmbeddings = await GenerateEmbeddingsAsync(chunks, cancellationToken);
            embeddings.AddRange(docEmbeddings);
        }
        
        return embeddings;
    }
}
```

## Integration Points

### 1. Gateway Discovery
- Provider exposes `/manifest` endpoint
- Gateway auto-discovers LlamaIndex capabilities
- Supports RAG, indexing, and search operations

### 2. Request Routing
- Requests with `type: "rag"` route to LlamaIndex provider
- Smart routing can analyze query complexity
- Falls back to other providers if needed

### 3. Model Support
- OpenAI models (GPT-4, GPT-3.5-turbo)
- Local models via Ollama
- Azure OpenAI integration

## Configuration

### LlamaIndexOptions.cs
```csharp
public class LlamaIndexOptions
{
    public string DefaultIndexPath { get; set; } = "./indexes";  // Default fallback
    public string DefaultModel { get; set; } = "gpt-3.5-turbo";
    public string EmbeddingModel { get; set; } = "text-embedding-ada-002";
    public int DefaultTopK { get; set; } = 5;
    public int ChunkSize { get; set; } = 1000;
    public int ChunkOverlap { get; set; } = 200;
    
    // Vector store agnostic - configured via IndexPath in request
    public Dictionary<string, VectorStoreConfig> VectorStores { get; set; } = new();
}

public class VectorStoreConfig
{
    public string Type { get; set; } = string.Empty;  // "postgresql", "neo4j", "chroma", etc.
    public string ConnectionString { get; set; } = string.Empty;
    public Dictionary<string, object> Settings { get; set; } = new();
}
```

## API Endpoints

### 1. Manifest Discovery
```
GET /manifest
```

### 2. RAG Operations
```
POST /api/rag/execute
```

### 3. Document Management
```
POST /api/documents/index
GET /api/documents/{indexId}
DELETE /api/documents/{indexId}
```

## Implementation Phases

### Phase 1: Core RAG
- Basic query-response with retrieval
- Local vector storage
- Single model support

### Phase 2: Advanced Features
- Multiple vector stores
- Document management API
- Model selection

### Phase 3: Enterprise Features
- Multi-tenant indexing
- Advanced chunking strategies
- Performance optimization

## Technology Stack

- **Framework**: .NET 9 with minimal APIs
- **LlamaIndex**: Python LlamaIndex via HTTP API
- **Vector Store**: ChromaDB (default), Pinecone (production)
- **Embeddings**: OpenAI text-embedding-ada-002
- **Models**: OpenAI GPT models, local via Ollama

## Expected Usage

```csharp
// Gateway routes this to LlamaIndex provider
var response = await kernel.ExecuteAsync(new RagRequest
{
    Query = "What are the security best practices for API development?",
    IndexId = "company-docs",
    TopK = 3,
    Model = "gpt-4"
});
```

This design ensures the LlamaIndex provider is specialized for RAG operations while integrating seamlessly with the Care.AI.Kernel gateway pattern.