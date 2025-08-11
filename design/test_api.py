#!/usr/bin/env python3
"""
Simple test script for Care.AI API
Usage: python test_api.py
"""

import requests
import json
from datetime import datetime

# API base URL
BASE_URL = "http://localhost:5001"

def test_health():
    """Test API health endpoint"""
    print("Testing API health...")
    try:
        response = requests.get(f"{BASE_URL}/test")
        print(f"✓ Health check: {response.status_code}")
        if response.headers.get('content-type', '').startswith('application/json'):
            print(f"  Response: {response.json()}")
        else:
            print(f"  Response: {response.text}")
        return response.status_code == 200
    except Exception as e:
        print(f"✗ Health check failed: {e}")
        return False

def test_chat():
    """Test chat endpoint"""
    print("\nTesting chat endpoint...")
    try:
        payload = {
            "userId": "test-user-123",
            "message": "Hello, how are you?",
            "sessionId": f"session-{datetime.now().timestamp()}"
        }
        response = requests.post(
            f"{BASE_URL}/chat",
            json=payload,
            headers={"Content-Type": "application/json"}
        )
        print(f"✓ Chat request: {response.status_code}")
        print(f"  Response: {response.json()}")
        return True
    except Exception as e:
        print(f"✗ Chat request failed: {e}")
        return False

def test_user_function():
    """Test user function"""
    print("\nTesting user function...")
    try:
        payload = {
            "userId": "test-user-123",
            "function": "User.GetUserProfile",
            "args": {"userId": "test-user-123"}
        }
        response = requests.post(
            f"{BASE_URL}/execute",
            json=payload,
            headers={
                "Content-Type": "application/json",
                "X-User-Type": "user",
                "X-User-Id": "test-user-123"
            }
        )
        print(f"✓ User function: {response.status_code}")
        if response.status_code == 200:
            print(f"  Response: {response.json()}")
        return response.status_code == 200
    except Exception as e:
        print(f"✗ User function failed: {e}")
        return False

def test_kernel_execute():
    """Test kernel execute endpoint"""
    print("\nTesting kernel execute...")
    try:
        payload = {
            "userId": "test-user-123",
            "function": "TestPlugin.TestFunction",
            "args": {}
        }
        response = requests.post(
            f"{BASE_URL}/execute",
            json=payload,
            headers={
                "Content-Type": "application/json",
                "X-User-Type": "user",
                "X-User-Id": "test-user-123"
            }
        )
        print(f"✓ Kernel execute: {response.status_code}")
        if response.status_code == 200:
            print(f"  Response: {response.json()}")
        return response.status_code == 200
    except Exception as e:
        print(f"✗ Kernel execute failed: {e}")
        return False

def test_admin_function():
    """Test admin-only function"""
    print("\nTesting admin function...")
    try:
        # Try as regular user (should fail)
        payload = {
            "userId": "regular-user",
            "function": "Admin.GetSystemInfo",
            "args": {}
        }
        response = requests.post(
            f"{BASE_URL}/admin/execute",
            json=payload,
            headers={
                "Content-Type": "application/json",
                "X-User-Type": "user",
                "X-User-Id": "regular-user"
            }
        )
        print(f"  Regular user attempt: {response.status_code} (should be 403)")
        
        # Try as admin (should succeed)
        response = requests.post(
            f"{BASE_URL}/admin/execute",
            json=payload,
            headers={
                "Content-Type": "application/json",
                "X-User-Type": "admin",
                "X-User-Id": "admin-user"
            }
        )
        print(f"✓ Admin access: {response.status_code}")
        if response.status_code == 200:
            print(f"  Response: {response.json()}")
        return response.status_code == 200
    except Exception as e:
        print(f"✗ Admin function failed: {e}")
        return False

def test_workflow_yaml():
    """Test workflow YAML execution"""
    print("\nTesting workflow YAML execution...")
    try:
        yaml_workflow = """Name: TestWorkflow
Steps:
  - Id: validate
    Type: code
    Ref: ValidateInput
    
  - Id: process
    Type: ai
    Prompt: "Process this data: {{data}}"
    
  - Id: save
    Type: code
    Ref: SaveToDatabase"""
        
        payload = {
            "yaml": yaml_workflow,
            "context": {
                "data": "test data"
            }
        }
        response = requests.post(
            f"{BASE_URL}/api/workflow/execute-yaml",
            json=payload,
            headers={
                "Content-Type": "application/json",
                "X-User-Type": "user",
                "X-User-Id": "test-user"
            }
        )
        print(f"✓ Workflow YAML execution: {response.status_code}")
        if response.status_code == 200:
            result = response.json()
            print(f"  Status: {result.get('status')}")
            print(f"  Outputs: {list(result.get('outputs', {}).keys())}")
        
        return response.status_code == 200
    except Exception as e:
        print(f"✗ Workflow YAML execution failed: {e}")
        return False

def test_workflow_crud():
    """Test workflow CRUD operations"""
    print("\nTesting workflow CRUD operations...")
    try:
        # Create workflow
        workflow = {
            "name": "Test Workflow",
            "description": "Test workflow for API testing",
            "steps": [
                {
                    "Id": "step1",
                    "Name": "First Step",
                    "Type": "code",
                    "FunctionId": "TestFunction"
                }
            ]
        }
        
        # Save workflow
        response = requests.post(
            f"{BASE_URL}/api/workflow",
            json=workflow,
            headers={
                "Content-Type": "application/json",
                "X-User-Type": "user",
                "X-User-Id": "test-user"
            }
        )
        print(f"✓ Create workflow: {response.status_code}")
        if response.status_code != 200:
            return False
            
        workflow_id = response.json().get('id')
        print(f"  Created workflow ID: {workflow_id}")
        
        # Get workflow
        response = requests.get(
            f"{BASE_URL}/api/workflow/{workflow_id}",
            headers={
                "X-User-Type": "user",
                "X-User-Id": "test-user"
            }
        )
        print(f"✓ Get workflow: {response.status_code}")
        
        # Delete workflow
        response = requests.delete(
            f"{BASE_URL}/api/workflow/{workflow_id}",
            headers={
                "X-User-Type": "user",
                "X-User-Id": "test-user"
            }
        )
        print(f"✓ Delete workflow: {response.status_code}")
        
        return response.status_code == 204
    except Exception as e:
        print(f"✗ Workflow CRUD failed: {e}")
        return False

def test_workflow_action():
    """Test workflow action endpoints"""
    print("\nTesting workflow action endpoints...")
    try:
        # Create workflow action
        payload = {
            "name": "Action Test Workflow",
            "description": "Created via action endpoint",
            "steps": [
                {
                    "Id": "step1",
                    "Name": "Test Step",
                    "Type": "ai",
                    "Prompt": "Test prompt"
                }
            ]
        }
        response = requests.post(
            f"{BASE_URL}/api/workflow/action/create",
            json=payload,
            headers={
                "Content-Type": "application/json",
                "X-User-Type": "user",
                "X-User-Id": "test-user"
            }
        )
        print(f"✓ Create workflow action: {response.status_code}")
        if response.status_code == 200:
            data = response.json()
            print(f"  Workflow ID: {data.get('id')}")
            
            # Schedule workflow
            schedule_payload = {
                "workflowId": data.get('id'),
                "schedule": "daily"
            }
            response = requests.post(
                f"{BASE_URL}/api/workflow/action/schedule",
                json=schedule_payload,
                headers={
                    "Content-Type": "application/json",
                    "X-User-Type": "user",
                    "X-User-Id": "test-user"
                }
            )
            print(f"✓ Schedule workflow action: {response.status_code}")
        
        return response.status_code == 200
    except Exception as e:
        print(f"✗ Workflow action failed: {e}")
        return False


def main():
    """Run all tests"""
    print("=" * 50)
    print("Care.AI API Test Suite")
    print("=" * 50)
    print(f"Target: {BASE_URL}")
    print(f"Time: {datetime.now()}\n")
    
    tests = [
        test_health,
        test_chat,
        test_kernel_execute,
        test_user_function,
        test_admin_function,
        test_workflow_yaml,
        test_workflow_crud,
        test_workflow_action
    ]
    
    passed = 0
    for test in tests:
        if test():
            passed += 1
    
    print("\n" + "=" * 50)
    print(f"Results: {passed}/{len(tests)} tests passed")
    print("=" * 50)
    
    return passed == len(tests)

if __name__ == "__main__":
    import sys
    success = main()
    sys.exit(0 if success else 1)