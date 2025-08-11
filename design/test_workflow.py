import yaml
import asyncio
from typing import Dict, Any, Optional

class WorkflowRunner:
    def __init__(self):
        self.context: Dict[str, Any] = {}
    
    async def run_yaml(self, yaml_path: str, initial_context: Dict[str, Any] = None) -> Dict[str, Any]:
        """Run workflow from YAML file"""
        with open(yaml_path, 'r') as f:
            workflow = yaml.safe_load(f)
        
        if initial_context:
            self.context.update(initial_context)
        
        return await self.run(workflow)
    
    async def run(self, workflow: dict) -> Dict[str, Any]:
        """Execute workflow steps"""
        print(f"\n🚀 Running workflow: {workflow['Name']}")
        
        for step in workflow['Steps']:
            if not self._should_execute(step.get('When')):
                print(f"⏭️  Skipping step '{step['Id']}' (condition not met)")
                continue
            
            print(f"\n📍 Executing step '{step['Id']}' (type: {step['Type']})")
            
            result = await self._execute_step(step)
            self.context[step['Id']] = result
            print(f"✅ Step '{step['Id']}' completed: {result}")
        
        return self.context
    
    def _should_execute(self, condition: Optional[str]) -> bool:
        """Evaluate simple conditions"""
        if not condition:
            return True
        
        # Simple "key == 'value'" evaluation
        if '==' in condition:
            parts = condition.split('==')
            if len(parts) == 2:
                key = parts[0].strip()
                expected = parts[1].strip().strip("'\"")
                actual = self.context.get(key)
                return str(actual) == expected
        
        return True
    
    async def _execute_step(self, step: dict) -> Any:
        """Execute a single workflow step"""
        step_type = step['Type']
        
        if step_type == 'ai':
            return await self._execute_ai(step)
        elif step_type == 'human':
            return await self._execute_human(step)
        elif step_type == 'workflow':
            return await self._execute_workflow(step)
        elif step_type == 'code':
            return await self._execute_code(step)
        else:
            raise ValueError(f"Unknown step type: {step_type}")
    
    async def _execute_ai(self, step: dict) -> str:
        """Mock AI execution"""
        prompt = step.get('prompt', '')
        
        # Replace variables in prompt
        for key, value in self.context.items():
            prompt = prompt.replace(f"{{{{{key}}}}}", str(value))
        
        print(f"  AI Prompt: {prompt}")
        
        # Mock responses based on prompt content
        if "urgency" in prompt.lower():
            return "high" if "severe" in self.context.get('symptoms', '').lower() else "low"
        elif "valid" in prompt.lower():
            return "yes"
        elif "entities" in prompt.lower():
            return "entities: person, location, date"
        
        return "ai_response"
    
    async def _execute_human(self, step: dict) -> str:
        """Mock human approval"""
        assignee = step.get('assign', 'user')
        print(f"  ⏳ Waiting for approval from: {assignee}")
        await asyncio.sleep(0.5)  # Simulate wait
        return f"approved_by_{assignee}"
    
    async def _execute_workflow(self, step: dict) -> str:
        """Mock sub-workflow execution"""
        ref = step.get('ref', 'unknown')
        print(f"  🔄 Running sub-workflow: {ref}")
        await asyncio.sleep(0.3)
        return f"workflow_{ref}_completed"
    
    async def _execute_code(self, step: dict) -> str:
        """Mock code execution"""
        ref = step.get('ref', 'unknown')
        print(f"  💻 Executing code: {ref}")
        
        # Mock different code functions
        if ref == 'ValidateInput':
            return "validation_passed"
        elif ref == 'SendNotification':
            return "notification_sent"
        elif ref == 'SaveToDatabase':
            return "data_saved"
        
        return f"code_{ref}_executed"


async def test_workflows():
    """Test all workflow examples"""
    runner = WorkflowRunner()
    
    # Test 1: Patient Care Workflow
    print("\n" + "="*50)
    print("TEST 1: Patient Care Workflow")
    print("="*50)
    
    context = await runner.run_yaml(
        "../src/Care.AI.Api/Workflows/patient-care.yaml",
        {"symptoms": "severe headache and fever"}
    )
    print(f"\nFinal context: {context}")
    
    # Test 2: Simple Approval Workflow
    print("\n" + "="*50)
    print("TEST 2: Simple Approval Workflow")
    print("="*50)
    
    runner = WorkflowRunner()  # Fresh context
    context = await runner.run_yaml(
        "../src/Care.AI.Api/Workflows/simple-approval.yaml",
        {"request": "purchase order #123"}
    )
    print(f"\nFinal context: {context}")
    
    # Test 3: Data Processing Workflow
    print("\n" + "="*50)
    print("TEST 3: Data Processing Workflow")
    print("="*50)
    
    runner = WorkflowRunner()  # Fresh context
    context = await runner.run_yaml(
        "../src/Care.AI.Api/Workflows/data-processing.yaml",
        {"data": "John Doe visited New York on 2024-01-15"}
    )
    print(f"\nFinal context: {context}")


if __name__ == "__main__":
    asyncio.run(test_workflows())