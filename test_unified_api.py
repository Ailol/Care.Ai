#!/usr/bin/env python3
import requests
import json
import sys

# Configuration
API_BASE_URL = "http://localhost:51353/api/kernel"
HEADERS = {"Content-Type": "application/json"}

def test_get_capabilities():
    """Test GET /capabilities endpoint - unified view"""
    print("\n1. Testing GET /capabilities (Unified View)...")
    try:
        response = requests.get(f"{API_BASE_URL}/capabilities", headers=HEADERS)
        print(f"Status: {response.status_code}")
        if response.status_code == 200:
            print(f"Unified Capabilities: {json.dumps(response.json(), indent=2)}")
        else:
            print(f"Error: {response.text}")
    except Exception as e:
        print(f"Error: {e}")
        return False
    return True

def test_execute_without_provider():
    """Test POST /execute without specifying provider - kernel decides"""
    print("\n2. Testing POST /execute (Kernel Auto-Selection)...")
    payload = {
        "type": "chat-completion",
        "data": {
            # No provider specified - kernel will choose based on priority
            "messages": [
                {"role": "user", "content": "What is 2+2?"}
            ],
            "maxTokens": 50,
            "temperature": 0.7
        }
    }
    try:
        response = requests.post(f"{API_BASE_URL}/execute", json=payload, headers=HEADERS, timeout=70)
        print(f"Status: {response.status_code}")
        result = response.json()
        print(f"Response: {json.dumps(result, indent=2)}")
        print("\nNote: Kernel automatically selected the best provider!")
    except Exception as e:
        print(f"Error: {e}")

def test_execute_with_model():
    """Test POST /execute with model - kernel finds compatible provider"""
    print("\n3. Testing POST /execute (Model-based Selection)...")
    payload = {
        "type": "chat-completion",
        "data": {
            "model": "qwen3-clean:latest",  # Kernel will find provider supporting this model
            "messages": [
                {"role": "user", "content": "Hello, how are you?"}
            ],
            "maxTokens": 50,
            "temperature": 0.7
        }
    }
    try:
        response = requests.post(f"{API_BASE_URL}/execute", json=payload, headers=HEADERS, timeout=70)
        print(f"Status: {response.status_code}")
        result = response.json()
        print(f"Response: {json.dumps(result, indent=2)}")
        print("\nNote: Kernel found a provider that supports the requested model!")
    except Exception as e:
        print(f"Error: {e}")

def test_execute_with_smart_routing():
    """Test POST /execute with smart routing enabled"""
    print("\n4. Testing POST /execute (Smart Routing)...")
    payload = {
        "type": "chat-completion",
        "data": {
            "useSmartRouting": True,  # Let AI decide the best provider
            "messages": [
                {"role": "user", "content": "Explain quantum computing in simple terms"}
            ],
            "maxTokens": 100,
            "temperature": 0.7
        }
    }
    try:
        response = requests.post(f"{API_BASE_URL}/execute", json=payload, headers=HEADERS, timeout=70)
        print(f"Status: {response.status_code}")
        result = response.json()
        print(f"Response: {json.dumps(result, indent=2)}")
        print("\nNote: Smart routing analyzed the request and chose the best provider!")
    except Exception as e:
        print(f"Error: {e}")

def test_get_providers_detail():
    """Test GET /providers for detailed view (optional)"""
    print("\n5. Testing GET /providers (Detailed View - Optional)...")
    try:
        response = requests.get(f"{API_BASE_URL}/providers", headers=HEADERS)
        print(f"Status: {response.status_code}")
        if response.status_code == 200:
            providers = response.json()
            print(f"Found {len(providers)} providers:")
            for p in providers:
                print(f"  - {p['providerId']}: {', '.join(p['operations'])}")
    except Exception as e:
        print(f"Error: {e}")

def main():
    print("Care.AI.Kernel - Unified AI Gateway Demo")
    print("=" * 50)
    print("The kernel acts as a unified interface to multiple AI providers.")
    print("The API doesn't need to know which providers exist!")
    print("=" * 50)
    
    # Test unified capabilities first
    if not test_get_capabilities():
        print("\nCannot connect to API. Please ensure Care.Ai.Api is running.")
        print("Expected URL: http://localhost:51353")
        sys.exit(1)
    
    # Show different ways the kernel unifies providers
    test_execute_without_provider()
    test_execute_with_model()
    test_execute_with_smart_routing()
    
    # Optional: show provider details
    print("\n" + "-" * 50)
    print("Optional: Provider Details")
    test_get_providers_detail()
    
    print("\n" + "=" * 50)
    print("Summary:")
    print("- The kernel provides a unified interface to all AI capabilities")
    print("- Clients can use operations without knowing about providers")
    print("- The kernel automatically routes to the best provider")
    print("- Smart routing can analyze requests to choose optimal providers")
    print("=" * 50)

if __name__ == "__main__":
    main()