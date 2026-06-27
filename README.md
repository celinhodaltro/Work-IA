# AI Office OS 🏢

Multi-agent engineering coordination platform built on .NET 8 with Clean Architecture, CQRS (via MediatR pipeline), and DDD.

## Features

- **Multi-Agent Workforce** — 9 agent roles (Head of Engineering, Tech Leads, QA, Reviewers, etc.)
- **Event-Driven Architecture** — Real-time event processing via InMemory Event Bus
- **Workspace Adapter** — Pluggable adapters for OpenCode, Claude Code, VS Code
- **Memory System** — Persistent learning from successes and failures (SQLite + Audit Trail)
- **Real-Time Dashboard** — Blazor WASM UI with MudBlazor
- **VS Code Extension** — Agent visualization in your IDE
- **Plugin System** — Extensible adapter architecture
- **SQLite Storage** — Portable, zero-configuration database

## Architecture

```
src/
├── 1 - Presentation/     Blazor Dashboard, Web API, CLI, VS Code Extension
├── 2 - Application/      CQRS Commands/Queries (MediatR), Behaviors, Services
├── 3 - Domain/           Entities, Aggregates, Value Objects, Domain Events
└── 4 - Infrastructure/   Persistence, Adapters, Event Bus, Cache
```

## Quick Start

### Prerequisites
- .NET 8 SDK
- VS Code (optional, for extension)
- Docker Desktop (optional, for RabbitMQ/Redis)

### Run Backend
```bash
git clone https://github.com/celinhodaltro/Work-IA.git
cd Work-IA
dotnet run --project "src/1 - Presentation/Work-IA.WebApi"
```

Open browser at http://localhost:5000/swagger

### Run Dashboard
```bash
dotnet run --project "src/1 - Presentation/Work-IA.BlazorDashboard"
```

### Run Tests
```bash
dotnet test
```

### Build VS Code Extension
```bash
cd "src/1 - Presentation/work-ia-vscode"
npm install
npm run compile
```

### Environment Variables (optional)
| Variable | Default | Description |
|----------|---------|-------------|
| `EventBus__Provider` | InMemory | InMemory or RabbitMQ |
| `ConnectionStrings__Default` | Data Source=work-ia.db | SQLite connection string |

## Project Structure

### Domain Layer
Work-IA.Domain — Entities, Aggregates, Value Objects, Domain Events

### Application Layer
Work-IA.Application — CQRS Commands/Queries via MediatR, Validation Behaviors, Services

### Infrastructure Layer
Work-IA.Infrastructure — EF Core SQLite, InMemory Event Bus, Adapters, File System, Git, Memory Store

### Presentation Layer
- Work-IA.WebApi — ASP.NET Core API, SignalR Hubs, Swagger
- Work-IA.BlazorDashboard — MudBlazor WASM Dashboard
- Work-IA.CLI — Command-line interface
- work-ia-vscode — VS Code Extension (TypeScript)

## Agent Roles

| Role | Responsibility |
|------|---------------|
| Head of Engineering | Coordinates all tech leads, receives consolidated reports |
| Tech Lead Backend | Observes .NET files, delegates to specialists |
| Tech Lead Frontend | Observes Blazor/razor files |
| Tech Lead Game | Observes game engine files |
| Tech Lead DevOps | Monitors builds and deployments |
| Test Lead | Schedules tests, monitors coverage |
| Chief Reviewer | Blocks deliveries when quality fails |
| Audit Lead | Records lessons learned |
| Architect | Reviews architecture decisions (veto power) |

## Tech Stack

- **Language:** C# 12 (.NET 8)
- **Architecture:** Clean Architecture + CQRS (MediatR) + DDD
- **Frontend:** Blazor WebAssembly + MudBlazor
- **Database:** SQLite (portable)
- **Real-Time:** SignalR
- **Event Bus:** InMemory (default), RabbitMQ (optional — not connected)
- **Event Store:** Audit Trail via SQLite (not Event Sourcing — no replay)
- **IDE Integration:** VS Code Extension (TypeScript + SignalR)
- **Tests:** xUnit + FluentAssertions + Moq
- **CI:** GitHub Actions

## Links

- Repository: https://github.com/celinhodaltro/Work-IA
- Pixel Agent (UX Reference): https://github.com/pixel-agents-hq/pixel-agents

## License

MIT
