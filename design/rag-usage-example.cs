// Example of using RAG service through the kernel

// 1. Through the kernel's IRagService interface (direct access)
var kernel = serviceProvider.GetRequiredService<IKernel>();

if (kernel.RagService != null)
{
    // Ingest documents
    var ingestionResult = await kernel.RagService.IngestAsync(
        collectionName: "product-docs",
        documents: new[]
        {
            "Care.AI.Kernel is a unified gateway for AI orchestration",
            "It supports multiple AI frameworks like Semantic Kernel and RAG",
            "RAG enables retrieval-augmented generation for better context"
        },
        metadata: new Dictionary<string, object> { ["source"] = "docs" }
    );
    
    Console.WriteLine($"Ingested {ingestionResult.DocumentsIngested} documents");
    
    // Query documents
    var queryResult = await kernel.RagService.QueryAsync(
        collectionName: "product-docs",
        query: "What is Care.AI.Kernel?",
        topK: 3,
        scoreThreshold: 0.7f
    );
    
    foreach (var doc in queryResult.Documents)
    {
        Console.WriteLine($"Score: {doc.Score:F2} - {doc.Content}");
    }
}

// 2. Through the kernel's request/response pattern (for routing)
var request = new KernelRequest
{
    Type = "rag",
    Data = new Dictionary<string, object>
    {
        ["mode"] = "query",
        ["collectionName"] = "product-docs",
        ["query"] = "What is Care.AI.Kernel?",
        ["topK"] = 3
    }
};

var response = await kernel.ExecuteAsync(request);