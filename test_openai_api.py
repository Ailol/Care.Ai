#!/usr/bin/env python3
import requests
import json

# Test OpenAI-compatible endpoints
base_url = "http://localhost:51353/v1"

print("Testing OpenAI-compatible API for OpenWebUI")
print("=" * 50)

# Test 1: Models endpoint
print("\n1. Testing /v1/models endpoint:")
try:
    response = requests.get(f"{base_url}/models")
    print(f"Status: {response.status_code}")
    if response.status_code == 200:
        data = response.json()
        print(f"Response structure: {list(data.keys())}")
        if 'data' in data:
            print(f"Found {len(data['data'])} models:")
            for model in data['data']:
                print(f"  - {model['id']}")
    else:
        print(f"Error: {response.text}")
except Exception as e:
    print(f"Error: {e}")

# Test 2: Chat completions
print("\n2. Testing /v1/chat/completions endpoint:")
try:
    payload = {
        "model": "qwen3-clean:latest",
        "messages": [
            {"role": "user", "content": "Say hello"}
        ]
    }
    response = requests.post(f"{base_url}/chat/completions", json=payload)
    print(f"Status: {response.status_code}")
    if response.status_code == 200:
        data = response.json()
        print(f"Response: {json.dumps(data, indent=2)}")
    else:
        print(f"Error: {response.text}")
except Exception as e:
    print(f"Error: {e}")

print("\n" + "=" * 50)
print("To use with OpenWebUI:")
print("1. Go to Settings → Connections")
print("2. Add OpenAI-compatible API")
print(f"3. API Base URL: {base_url}")
print("4. API Key: any-string-will-work")
print("=" * 50)