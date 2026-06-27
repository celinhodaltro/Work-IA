# Event Flow

## Architecture Overview

The event system has three distinct pipelines:

```
External Event
      │
      ▼
Workspace Adapter ──→ Integration Event ──→ InMemoryEventBus
      │                                         │
      │                                    ┌────┴────┐
      │                                    ▼         ▼
      │                              MediatR      SignalR
      │                              Handlers     Hub Broadcast
      │
      ▼
Domain Entity ──→ Domain Event ──→ IDomainEventDispatcher
                                          │
                                    ┌─────┴──────┐
                                    ▼            ▼
                              Event Store    Domain Event
                              (Audit Trail)  Handlers
```

## Pipeline 1: External Events → Integration Events

1. External stimulus (file change, git commit, tool event)
2. `FileSystemWorkspaceAdapter` or `OpenCodeAdapter` detects the change
3. Adapter creates an `IIntegrationEvent` (e.g., `WorkspaceFileChange`)
4. Published to `InMemoryEventBus`

## Pipeline 2: Integration Events → Agent Processing

1. `InMemoryEventBus.PublishAsync<T>()` fans out to all subscribed handlers
2. Handlers include MediatR notification handlers and direct subscribers
3. Agents receive events and process according to their ObservationRules
4. Results broadcast via SignalR to connected dashboards/extensions

## Pipeline 3: Domain Events → Event Store

1. Domain entities raise domain events (e.g., `WorkflowStartedDomainEvent`)
2. `IDomainEventDispatcher` collects and dispatches all pending events
3. Events are persisted to the **Event Store** (SQLite Audit Trail)
4. Domain event handlers execute side effects (if any)

## Key Components

| Component | File | Description |
|-----------|------|-------------|
| **InMemoryEventBus** | `Infrastructure/EventBus/InMemoryEventBus.cs` | Concurrent in-memory pub/sub with dead letter queue |
| **IDomainEventDispatcher** | `Application/Common/Interfaces/IDomainEventDispatcher.cs` | Abstraction for dispatching domain events |
| **IEventStore** | `Application/Common/Interfaces/IEventStore.cs` | Abstraction for persisting events |
| **IEventBus** | `Application/Common/Interfaces/IEventBus.cs` | Abstraction for integration event pub/sub |
| **WorkspaceFileChange** | `Application/Common/Events/WorkspaceFileChange.cs` | Integration event for file changes |

## Dead Letter Queue

Failed event handlers are tracked in the `DeadLetterQueue` — events that threw exceptions are stored with exception details for later inspection.
