# Architecture Overview

## High-Level Architecture

AI Office OS follows Clean Architecture with 4 layers:

### Dependency Rule
Dependencies point inward: Presentation → Application → Domain ← Infrastructure

### Event Flow
1. External event (file change, git commit)
2. Workspace Adapter converts to Integration Event
3. Event Bus (InMemory) publishes to subscribed agents/handlers
4. Agents process and react
5. Decisions recorded in Memory Store (SQLite)
6. Real-time UI updates via SignalR

### Key Patterns
- **CQRS (MediatR Pipeline):** Commands for writes, Queries for reads — pipeline configured with Logging and Validation behaviors
- **Domain Events:** Pure C# records raised by domain entities, persisted in Event Store (Audit Trail)
- **Adapter Pattern:** `IWorkspaceAdapter` interface for IDE/Platform abstraction
- **Plugin System:** `IAdapterPlugin` for extensible adapters loaded from assemblies
- **Observer Pattern:** `IFileChangeObservable` / `IObservable<T>` for file system events
- **Event Bus:** `IEventBus` abstraction with InMemory implementation (Dead Letter Queue included)

### Architecture Decisions
- **No Event Sourcing:** The Event Store is an Audit Trail for tracking domain events, not a source of truth for replay
- **No RabbitMQ by default:** InMemoryEventBus is the only operational implementation; RabbitMQ.Client dependency exists but is not connected
- **Horizontal Clean Architecture:** Not Vertical Slices — layers separate concerns by responsibility

### Layer Responsibilities

| Layer | Responsibility |
|-------|---------------|
| **Domain** | Enterprise business rules, entities, aggregates, value objects, domain events |
| **Application** | Application business rules, CQRS commands/queries, DTOs, mapping, validation |
| **Infrastructure** | Persistence (EF Core SQLite), Event Bus, Adapters, File System, Git, Memory |
| **Presentation** | Web API, Blazor WASM Dashboard, CLI, VS Code Extension |
