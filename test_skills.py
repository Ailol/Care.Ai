#!/usr/bin/env python3
import requests
import json
import sys

# Configuration
API_BASE_URL = "http://localhost:51353/api/kernel"
HEADERS = {"Content-Type": "application/json"}

def test_provider_skills():
    """Test providers with skills - shows internal routing logic"""
    print("\n1. Testing Provider Skills (Debug View)...")
    try:
        response = requests.get(f"{API_BASE_URL}/providers", headers=HEADERS)
        if response.status_code == 200:
            providers = response.json()
            for p in providers:
                print(f"\nProvider: {p['providerId']}")
                print(f"  Priority: {p['priority']}")
                print(f"  Operations: {', '.join(p['operations'])}")
                if 'skills' in p and p['skills']:
                    print(f"  Skills:")
                    for key, value in p['skills'].items():
                        print(f"    - {key}: {value}")
    except Exception as e:
        print(f"Error: {e}")

def test_skill_based_routing():
    """Test how skills influence routing decisions"""
    print("\n2. Testing Skill-Based Routing...")
    
    # Test 1: Request that should go to data provider (if we had one)
    print("\n   a) RAG/Data request:")
    payload = {
        "type": "chat-completion",
        "data": {
            "messages": [
                {"role": "user", "content": "Search my documents for information about project X"}
            ]
        }
    }
    response = requests.post(f"{API_BASE_URL}/execute", json=payload, headers=HEADERS, timeout=70)
    if response.status_code == 200:
        print(f"      Response received (provider would be chosen based on skills)")
    
    # Test 2: Request that needs orchestration
    print("\n   b) Orchestration request:")
    payload = {
        "type": "chat-completion",
        "data": {
            "messages": [
                {"role": "user", "content": "Coordinate multiple tasks: analyze, summarize, and report"}
            ]
        }
    }
    response = requests.post(f"{API_BASE_URL}/execute", json=payload, headers=HEADERS, timeout=70)
    if response.status_code == 200:
        print(f"      Response received (semantic-kernel has orchestration skills)")

def test_future_capabilities():
    """Show how future providers would work"""
    print("\n3. Future Provider Integration Examples:")
    print("\n   When you add Qwen-Agent (coding):")
    print("   - Skills: {\"tech\": \"qwen-agent\", \"type\": \"coding\", \"languages\": [\"python\", \"js\"]}")
    print("   - Requests about code generation would automatically route there")
    
    print("\n   When you add Dapr-Agent (critical):")
    print("   - Skills: {\"tech\": \"dapr\", \"type\": \"critical\", \"transactional\": true}")
    print("   - Requests needing transactions would automatically route there")

def main():
    print("Care.AI.Kernel - Skills System Demo")
    print("=" * 50)
    print("Shows how providers advertise their skills for intelligent routing")
    print("=" * 50)
    
    test_provider_skills()
    test_skill_based_routing()
    test_future_capabilities()
    
    print("\n" + "=" * 50)
    print("Skills enable:")
    print("- Providers to advertise their strengths")
    print("- Kernel to make intelligent routing decisions")
    print("- Easy integration of new specialized providers")
    print("- Zero configuration for clients")
    print("=" * 50)

if __name__ == "__main__":
    main()