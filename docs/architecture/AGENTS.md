# Agent System

## Overview

AI Office OS implements a multi-agent workforce where each agent has a specific role, observation rules, and lifecycle.

## Agent Roles (14 roles)

| Role | Responsibility |
|------|---------------|
| **HeadOfEngineering** | Coordinates all tech leads, receives consolidated reports, single point of entry |
| **TechLeadBackend** | Observes .NET files, decomposes tasks, delegates to backend specialists |
| **TechLeadFrontend** | Observes Blazor/razor files, delegates to frontend specialists |
| **TechLeadGame** | Observes game engine files, delegates to game specialists |
| **TechLeadDevOps** | Monitors builds, deployments, CI/CD pipelines |
| **TestLead** | Schedules tests, monitors coverage, delegates to test specialists |
| **ChiefReviewer** | Blocks deliveries when quality fails, manages Review Board |
| **AuditLead** | Records lessons learned, manages Audit & Learning Division |
| **Architect** | Reviews architecture decisions, veto power over implementations |
| **Specialist** | Executes technical work delegated by Tech Leads |
| **Ceo** | Strategic oversight, business decisions |

## Agent Lifecycle

```
Created → Idle → Active (Observing) → Processing → Idle
                    ↓                      ↓
                 Paused                 Completed/Failed
```

### States
- **Created:** Agent instantiated, not yet started
- **Idle:** Waiting for events or tasks
- **Active (Observing):** Monitoring files/events per ObservationRules
- **Processing:** Executing a delegated task
- **Paused:** Temporarily suspended
- **Completed:** Task finished successfully
- **Failed:** Task finished with error (logged to Audit Trail)

## Observation Rules

Each agent defines `ObservationRule` entries that specify:
- **File patterns** to watch (e.g., `*.cs`, `*.razor`)
- **Event types** to subscribe to
- **Priority** (Critical, High, Medium, Low)

## Communication

- Agents communicate through the **Event Bus** (InMemory)
- Tech Leads receive domain events and delegate via the **TaskDelegationService**
- All decisions and results are recorded in the **Memory System**
