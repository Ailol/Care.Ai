# Care.AI.Kernel Python Tests

Simple Python scripts to test the Care.AI.Kernel API and integrations.

## Setup

1. Install dependencies:
```bash
pip install -r requirements.txt
```

2. Ensure services are running:
- Care.AI.Api on port 5000
- Ollama with embedding model (qwen3-embedding:0.6b) on port 11434
- PostgreSQL with pgvector extension or Supabase instance

## Available Tests

### 1. API Tests (`test_kernel_api.py`)
Tests the main kernel API endpoints:
- Get provider manifests
- Index documents for RAG
- Query indexed documents
- Smart routing
- Provider-specific requests

Run:
```bash
python test_kernel_api.py
```

### 2. Ollama Integration Tests (`test_ollama_integration.py`)
Tests Ollama embedding functionality:
- Connectivity check
- Embedding generation
- Semantic similarity comparison

Run:
```bash
python test_ollama_integration.py
```

### 3. RAG Service Tests (`test_rag_service.py`)
Tests RAG service with vector store integration:
- Vector store configuration
- Document ingestion with embeddings
- Vector similarity search queries
- Provider discovery

Run:
```bash
python test_rag_service.py
```

### 4. Vector Store Tests (`test_vector_store.py`)
Tests vector store operations:
- PostgreSQL/pgvector setup
- Supabase configuration
- Embedding generation
- Batch operations
- Collection management

Run:
```bash
python test_vector_store.py
```

## Quick Test Commands

```bash
# Test manifest endpoint
curl http://localhost:5000/manifests

# Test RAG ingestion with vector store
curl -X POST http://localhost:5000/execute \
  -H "Content-Type: application/json" \
  -d '{
    "type": "rag-ingestion",
    "data": {
      "collectionName": "product-docs",
      "documents": ["AI is amazing", "ML is powerful", "Vector databases enable similarity search"],
      "metadata": {"source": "test"}
    },
    "providerId": "rag-framework"
  }'

# Test RAG query with vector search
curl -X POST http://localhost:5000/execute \
  -H "Content-Type: application/json" \
  -d '{
    "type": "rag-query",
    "data": {
      "collectionName": "product-docs",
      "query": "What is AI and vector search?",
      "topK": 3,
      "scoreThreshold": 0.5
    },
    "providerId": "rag-framework"
  }'
```

## Expected Results

- Manifests should list at least 2 providers (semantic-kernel, rag-framework)
- RAG ingestion should store documents in PostgreSQL/Supabase with embeddings
- RAG queries should return relevant documents ordered by vector similarity
- Ollama should generate 768-dimensional embeddings using qwen3-embedding:0.6b
- Vector similarity scores should be between 0 and 1 (cosine similarity)
- PostgreSQL queries should use pgvector's HNSW index for fast retrieval

## Configuration

Add to `appsettings.json`:

```json
{
  "VectorStore": {
    "Provider": "PostgreSQL",
    "Url": "postgresql://user:password@localhost:5432/vectordb",
    // Or for Supabase:
    // "Url": "https://your-project.supabase.co",
    // "Key": "your-anon-key"
  },
  "Ollama": {
    "Endpoint": "http://localhost:11434/v1",
    "EmbeddingModel": "qwen3-embedding:0.6b"
  }
}
```