#!/usr/bin/env python3
import requests
import json

# Test what OpenWebUI is receiving
payload = {
    "model": "qwen3-clean:latest",
    "messages": [
        {"role": "user", "content": "Write a simple for loop"}
    ],
    "stream": False
}

response = requests.post("http://localhost:51353/v1/chat/completions", json=payload)
print("Raw response:")
print(json.dumps(response.json(), indent=2))

# Check the actual content
data = response.json()
if 'choices' in data and len(data['choices']) > 0:
    content = data['choices'][0]['message']['content']
    print("\nActual content:")
    print(repr(content))
    print("\nFormatted:")
    print(content)