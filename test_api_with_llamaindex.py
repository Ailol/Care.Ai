#!/usr/bin/env python3
import requests
import json
import sys

# Configuration
API_BASE_URL = "http://localhost:51353/api/kernel"
HEADERS = {"Content-Type": "application/json"}

def test_get_providers():
    """Test GET /providers endpoint"""
    print("\n1. Testing GET /providers...")
    try:
        response = requests.get(f"{API_BASE_URL}/providers", headers=HEADERS)
        print(f"Status: {response.status_code}")
        if response.status_code == 200:
            print(f"Response: {json.dumps(response.json(), indent=2)}")
        else:
            print(f"Error: {response.text}")
    except requests.exceptions.ConnectionError:
        print("Error: Could not connect to API. Is the service running?")
        return False
    except Exception as e:
        print(f"Error: {e}")
        return False
    return True

def test_execute_with_semantic_kernel():
    """Test POST /execute with Semantic Kernel provider"""
    print("\n2. Testing POST /execute with Semantic Kernel provider...")
    payload = {
        "type": "chat-completion",
        "data": {
            "provider": "semantic-kernel-provider",
            "messages": [
                {"role": "user", "content": "Say hello"}
            ],
            "maxTokens": 50,
            "temperature": 0.7
        }
    }
    try:
        response = requests.post(f"{API_BASE_URL}/execute", json=payload, headers=HEADERS, timeout=70)
        print(f"Status: {response.status_code}")
        print(f"Response: {json.dumps(response.json(), indent=2)}")
    except Exception as e:
        print(f"Error: {e}")

def test_execute_with_llamaindex():
    """Test POST /execute with LlamaIndex provider"""
    print("\n3. Testing POST /execute with LlamaIndex provider...")
    payload = {
        "type": "chat-completion",
        "data": {
            "provider": "llamaindex-provider",
            "messages": [
                {"role": "user", "content": "Say hello"}
            ],
            "maxTokens": 50,
            "temperature": 0.7
        }
    }
    try:
        response = requests.post(f"{API_BASE_URL}/execute", json=payload, headers=HEADERS, timeout=70)
        print(f"Status: {response.status_code}")
        print(f"Response: {json.dumps(response.json(), indent=2)}")
    except Exception as e:
        print(f"Error: {e}")

def test_semantic_kernel_health():
    """Test Semantic Kernel provider health endpoint"""
    print("\n4. Testing Semantic Kernel provider health...")
    try:
        response = requests.get("http://localhost:5050/health", headers=HEADERS, timeout=10)
        print(f"Status: {response.status_code}")
        if response.status_code == 200:
            print("Semantic Kernel provider is healthy")
    except Exception as e:
        print(f"Error: {e}")

def test_llamaindex_health():
    """Test LlamaIndex provider health endpoint"""
    print("\n5. Testing LlamaIndex provider health...")
    try:
        response = requests.get("http://localhost:5051/health", headers=HEADERS, timeout=10)
        print(f"Status: {response.status_code}")
        if response.status_code == 200:
            print("LlamaIndex provider is healthy")
    except Exception as e:
        print(f"Error: {e}")

def test_semantic_kernel_manifest():
    """Test Semantic Kernel provider manifest endpoint"""
    print("\n6. Testing Semantic Kernel provider manifest...")
    try:
        response = requests.get("http://localhost:5050/manifest", headers=HEADERS, timeout=10)
        print(f"Status: {response.status_code}")
        if response.status_code == 200:
            print(f"Manifest: {json.dumps(response.json(), indent=2)}")
    except Exception as e:
        print(f"Error: {e}")

def test_llamaindex_manifest():
    """Test LlamaIndex provider manifest endpoint"""
    print("\n7. Testing LlamaIndex provider manifest...")
    try:
        response = requests.get("http://localhost:5051/manifest", headers=HEADERS, timeout=10)
        print(f"Status: {response.status_code}")
        if response.status_code == 200:
            print(f"Manifest: {json.dumps(response.json(), indent=2)}")
    except Exception as e:
        print(f"Error: {e}")

def main():
    print("Care.Ai.Kernel API Test Suite - All Providers")
    print("=" * 50)
    print(f"Testing API at: {API_BASE_URL}")
    
    # Test connection first
    print("\nChecking API connectivity...")
    if not test_get_providers():
        print("\nCannot connect to API. Please ensure Care.Ai.Api is running.")
        print("Expected URL: http://localhost:51353")
        sys.exit(1)
    
    # Run tests for both providers
    test_execute_with_semantic_kernel()
    test_execute_with_llamaindex()
    test_semantic_kernel_health()
    test_llamaindex_health()
    test_semantic_kernel_manifest()
    test_llamaindex_manifest()
    
    print("\n" + "=" * 50)
    print("All tests completed!")

if __name__ == "__main__":
    main()