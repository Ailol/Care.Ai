#!/usr/bin/env python3
"""
Test script for RAG + LLM integration
"""

import requests
import json
import time

# API configuration
API_URL = "http://localhost:5000"
HEADERS = {"Content-Type": "application/json"}

def test_rag_with_llm():
    """Test RAG query with LLM answer generation"""
    print("\n" + "="*60)
    print("  Testing RAG + LLM Integration")
    print("="*60)
    
    test_queries = [
        "What is Care.AI.Kernel?",
        "How does vector embedding work?",
        "What databases are supported for vector storage?",
        "Explain the gateway pattern architecture"
    ]
    
    for query in test_queries:
        print(f"\n Query: '{query}'")
        
        request_data = {
            "type": "rag-query-with-llm",
            "data": {
                "collectionName": "test-collection",
                "query": query,
                "topK": 3
            }
        }
        
        try:
            start_time = time.time()
            response = requests.post(
                f"{API_URL}/api/kernel/execute",
                json=request_data,
                headers=HEADERS,
                timeout=60
            )
            elapsed = time.time() - start_time
            
            if response.status_code == 200:
                result = response.json()
                
                if result.get('success'):
                    answer = result.get('data', {}).get('answer', 'No answer')
                    sources = result.get('data', {}).get('sources', [])
                    metadata = result.get('metadata', {})
                    
                    print(f" ✓ Answer generated in {elapsed:.2f}s")
                    print(f" Documents retrieved: {metadata.get('documentsRetrieved', 0)}")
                    print(f" Answer: {answer[:200]}...")
                    
                    if sources:
                        print(f"\n Top sources:")
                        for i, source in enumerate(sources[:2]):
                            print(f"   [{i+1}] Score: {source['score']:.3f}")
                            print(f"       {source['content'][:100]}...")
                else:
                    print(f" ✗ Failed: {result.get('errorMessage', 'Unknown error')}")
            else:
                print(f" ✗ API error: {response.status_code}")
                print(f"   Response: {response.text[:200]}")
        except Exception as e:
            print(f" ✗ Error: {str(e)}")

def test_regular_rag():
    """Test regular RAG query (just retrieval)"""
    print("\n" + "="*60)
    print("  Testing Regular RAG Query (retrieval only)")
    print("="*60)
    
    request_data = {
        "type": "rag-query",
        "data": {
            "collectionName": "test-collection",
            "query": "What is Care.AI.Kernel?",
            "topK": 3
        }
    }
    
    try:
        response = requests.post(
            f"{API_URL}/api/kernel/execute",
            json=request_data,
            headers=HEADERS,
            timeout=30
        )
        
        if response.status_code == 200:
            result = response.json()
            
            if result.get('success'):
                documents = result.get('data', {}).get('documents', [])
                print(f" ✓ Retrieved {len(documents)} documents")
                
                for i, doc in enumerate(documents):
                    print(f"\n Document {i+1}:")
                    print(f"   Score: {doc.get('score', 0):.3f}")
                    print(f"   Content: {doc.get('content', '')[:150]}...")
            else:
                print(f" ✗ Failed: {result.get('errorMessage', 'Unknown error')}")
        else:
            print(f" ✗ API error: {response.status_code}")
    except Exception as e:
        print(f" ✗ Error: {str(e)}")

if __name__ == "__main__":
    # First test regular RAG to ensure it's working
    test_regular_rag()
    
    # Then test RAG + LLM integration
    test_rag_with_llm()
    
    print("\n" + "="*60)
    print("  Test Complete")
    print("="*60 + "\n")