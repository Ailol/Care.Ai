@echo off
echo Starting Dapr services (placement and scheduler)...
cd dapr-placement
docker-compose up -d
echo.
echo Dapr services are running:
echo   - Placement service on port 6050
echo   - Scheduler service on port 6060
echo You can now run your Aspire app from Visual Studio