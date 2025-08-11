@echo off
echo Starting Dapr placement service...
start /B dapr run --app-id placement --app-port 50005 --dapr-http-port 3501 -- placement -port 50005

timeout /t 3 /nobreak > nul

echo Starting Care.AI API with Dapr...
cd src\Care.AI.Api
dapr run --app-id care-ai-api --app-port 5001 --dapr-http-port 3500 --dapr-grpc-port 50001 --components-path ..\..\Care.Ai.AppHost\components -- dotnet run