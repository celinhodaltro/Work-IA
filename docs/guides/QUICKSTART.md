# Quick Start Guide

## 1. Clone and Build
```bash
git clone https://github.com/celinhodaltro/Work-IA.git
cd Work-IA
dotnet build
```

## 2. Run the API
```bash
dotnet run --project "src/1 - Presentation/Work-IA.WebApi"
```
API will be at http://localhost:5000

## 3. Verify Health
```bash
curl http://localhost:5000/health
```

## 4. Run the Dashboard
```bash
dotnet run --project "src/1 - Presentation/Work-IA.BlazorDashboard"
```
Dashboard will be at http://localhost:5500

## 5. Run the CLI (optional)
```bash
dotnet run --project "src/1 - Presentation/Work-IA.CLI" -- --help
```

## 6. Run Tests
```bash
dotnet test
```

## 7. VS Code Extension
```bash
cd "src/1 - Presentation/work-ia-vscode"
npm install
npm run compile
code .
# Press F5 to debug
```

## 8. Docker (optional)
```bash
docker compose up -d
# Starts supporting services (RabbitMQ, Redis if configured)
```

## Configuration
Configuration is read from `appsettings.json`. Key sections:

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=work-ia.db"
  },
  "EventBus": {
    "Provider": "InMemory"
  },
  "Adapters": {
    "FileSystem": {
      "Enabled": true,
      "WatchDirectory": "./src"
    }
  }
}
```
