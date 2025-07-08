from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import List, Dict, Any, Optional
from llama_index.llms.ollama import Ollama
import os
import logging

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Initialize FastAPI
app = FastAPI(title="LlamaIndex Provider for Care.AI")

# Configure Ollama
ollama_endpoint = os.getenv("OLLAMA_ENDPOINT", "http://ollama:11434")
model = os.getenv("OLLAMA_MODEL", "qwen3-clean:latest")

# Initialize LlamaIndex with Ollama
llm = Ollama(model=model, base_url=ollama_endpoint)

# Models
class Message(BaseModel):
    role: str
    content: str

class Request(BaseModel):
    type: str
    data: Dict[str, Any]
    metadata: Optional[Dict[str, Any]] = None

class Response(BaseModel):
    success: bool
    result: Optional[Any] = None
    errorMessage: Optional[str] = None

class KernelProviderManifest(BaseModel):
    providerId: str
    version: str
    availableModels: List[str]
    operations: List[str]
    serviceId: str
    executeEndpoint: str
    priority: int
    skills: Optional[Dict[str, Any]] = None

# Endpoints
@app.get("/manifest")
async def get_manifest():
    """Return provider manifest for discovery"""
    return KernelProviderManifest(
        providerId="llamaindex-provider",
        version="1.0.0",
        availableModels=["qwen3-clean:latest", "qwen3:0.6b"],
        operations=["chat-completion"],
        serviceId="llamaindex-provider",
        executeEndpoint="/execute",
        priority=85,  # Lower than Semantic Kernel
        skills={
            "tech": "llamaindex",
            "type": "data",
            "rag": True,
            "indexing": True,
            "vector_search": True
        }
    )

@app.post("/execute")
async def execute(request: Request):
    """Execute AI operations"""
    try:
        if request.type != "chat-completion":
            return Response(
                success=False,
                errorMessage=f"Operation '{request.type}' not supported. Only 'chat-completion' is available."
            )
        
        # Extract messages
        messages = []
        if "messages" in request.data:
            for msg in request.data["messages"]:
                if isinstance(msg, dict):
                    messages.append(Message(
                        role=msg.get("role", "user"),
                        content=msg.get("content", "")
                    ))
        
        if not messages:
            return Response(
                success=False,
                errorMessage="No messages provided in request"
            )
        
        # Get the last user message (LlamaIndex works with single prompts)
        last_user_message = None
        for msg in reversed(messages):
            if msg.role == "user":
                last_user_message = msg.content
                break
        
        if not last_user_message:
            return Response(
                success=False,
                errorMessage="No user message found in request"
            )
        
        # Call Ollama through LlamaIndex
        logger.info(f"Sending request to LlamaIndex with message: {last_user_message[:50]}...")
        
        # Use the LLM directly for chat
        response = llm.complete(last_user_message)
        
        return Response(
            success=True,
            result={
                "message": str(response),
                "role": "assistant"
            }
        )
        
    except Exception as e:
        logger.error(f"Error executing request: {str(e)}")
        return Response(
            success=False,
            errorMessage=f"Error executing request: {str(e)}"
        )

@app.get("/health")
async def health():
    """Health check endpoint"""
    return {"status": "Healthy"}

@app.get("/")
async def root():
    """Root endpoint"""
    return {"service": "LlamaIndex Provider", "version": "1.0.0"}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=5051)