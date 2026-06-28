# Análise Completa do AI Office OS — Diagnóstico + Plano de Refatoração

> **Data:** 27/06/2026
> **Projeto:** AI Office OS (Work-IA)
> **Repositório:** https://github.com/celinhodaltro/Work-IA

---

## Parte 1 — Diagnóstico Técnico Completo

### 1.1 Métricas Gerais

| Projeto | Arquivos .cs | Linhas de código | Propósito |
|---------|:-----------:|:----------------:|-----------|
| Work-IA.Domain | 75 | 1.464 | Entidades, Value Objects, Domain Events, Ports |
| Work-IA.Application | 94 | 1.500 | Commands, Queries, Handlers, Behaviors |
| Work-IA.Infrastructure | 50 | 2.473 | Persistência, Adapters, EventBus, Services |
| Work-IA.WebApi | 29 | 696 | Controllers, Hubs, Middleware |
| Work-IA.BlazorDashboard | 12 | 263 | Componentes Blazor WASM |
| Work-IA.CLI | 10 | 102 | CLI interface |
| Work-IA.Office | 14 | 689 | Silk.NET 3D Office |
| Work-IA.AppHost | 16 | 213 | Aspire Orchestrator |
| Work-IA.ServiceDefaults | 10 | 160 | Extensions + Aspire defaults |
| **TOTAL** | **310** | **7.560** | |

### 1.2 God Classes Identificadas (>100 linhas)

| Arquivo | Linhas | Problema |
|---------|:------:|----------|
| `OfficeRenderer.cs` | 307 | **GOD CLASS** — renderização + input + geometria tudo junto |
| `RoleSeedService.cs` | 280 | Dados inline. Deveria ser JSON externo |
| `AgentRepository.cs` | 147 | Mapping de domínio para entidade embutido |
| `WorkflowEngine.cs` | 143 | Registro + execução + workflows padrão tudo misturado |
| `FileSystemWatcherObserver.cs` | 133 | Lógica de watcher + debounce + ignore patterns |
| `OpenCodeAdapter.cs` | 130 | Adapter + serialização + event handling |
| `DatabaseMemoryStore.cs` | 120 | Store + mapping |
| `Agent.cs` | 120 | Aggregate rico, aceitável |
| `AgentBase.cs` | 119 | Base class com muitas responsabilidades |
| `OfficeHubClient.cs` | 109 | SignalR client, aceitável |

### 1.3 SRP Violations (Handlers com >4 dependências)

| Handler | Dependências | Problema |
|---------|:------------:|----------|
| CreateAgentCommandHandler | 6 (Repo, UoW, Registry, EventBus, Mediator, Logger) | Persiste + registra runtime — deveria ser 2 handlers |
| PromoteAgentCommandHandler | 6 | Handler + notificação + runtime update |
| CancelWorkflowCommandHandler | 5 | Executa + notifica |
| StartAgentCommandHandler | 5 | Persiste + broadcast |
| StopAgentCommandHandler | 5 | Persiste + broadcast |
| PauseAgentCommandHandler | 5 | Persiste + broadcast |

### 1.4 Problemas Estruturais

#### 1.4.1 ServiceDefaults — Projeto Inútil
`Work-IA.ServiceDefaults` tem **10 arquivos, 160 linhas** mas só UM arquivo é relevante (`Extensions.cs`). O resto são:
- `Properties/launchSettings.json` (desnecessário)
- `appsettings.json` (vazio)
- 7 arquivos de projetos/obj/assembly (lixo de compilação)

**Solução:** Mover `Extensions.cs` para Application ou Infrastructure. Remover projeto.

#### 1.4.2 Work-IA.CLI — Projeto Vazio
10 arquivos, 102 linhas. Não faz nada útil. Deveria ser removido ou ganhar funcionalidade real.

#### 1.4.3 Application Layer Inchaçada
94 arquivos para 1.500 linhas = média de 16 linhas/arquivo. Muitos arquivos minúsculos (Commands/Queries vazios). Isso é esperado em CQRS puro, mas indica que poderiam ser consolidados.

#### 1.4.4 Infrastructure Layer Inchada
50 arquivos, 2.473 linhas = média de 49 linhas/arquivo. Maior projeto. Mistura:
- Persistência (DbContext, Repositories, Entities, Configurations)
- EventBus (InMemoryEventBus)
- Adapters (OpenCode, ClaudeCode, FileSystem)
- Services (Background, Behavior, Seed)
- Comunicação (CommunicationBus)
- Workflows (WorkflowEngine)

#### 1.4.5 Domain — Namespace Confuso
Domain tem 75 arquivos mas espalhados em:
- `Agents/` — Agent, AgentId, AgentName, AgentTitle, AgentTask, etc.
- `Roles/` — RoleDefinition, RoleId (SEPARADO de Agents — erro!)
- `Abstractions/` — Entity, AggregateRoot, ValueObject
- `Events/` — Domain Events
- `Ports/` — Repository interfaces
- `Memory/` — MemoryEntry
- `Workflows/` — WorkflowDefinition, WorkflowInstance
- `Workspace/` — Workspace, WorkspaceFile

`RoleDefinition` deveria estar em `Agents/`, não em `Roles/` separado.

### 1.5 Clean Architecture Violations

| Violação | Local | Problema |
|----------|-------|----------|
| `IRoleRepository` em `Domain/Ports/`?? | Domain | Correto (porta de saída), mas confuso misturado com interfaces de Application |
| `IUnitOfWork` em Application/Common/Interfaces | Application | Correto, mas deveria estar em Domain como porta de saída |
| Handler com 6 dependências | Application | Viola Single Responsibility Principle |
| AgentBase recebe IMediator, IEventBus, ILogger | Application | Acoplamento excessivo |
| Mapping de domínio para entidade inline nos repositórios | Infrastructure | Viola Separation of Concerns |

### 1.6 CQRS Violations

- `CreateAgentCommandHandler` **persiste E registra runtime** — deveria apenas persistir. O registro runtime deveria ser um Domain Event Handler.
- AgentsController retorna `AgentDto` mapeado manualmente (sem AutoMapper consistente)
- Falta `UnitOfWorkBehavior` para gerenciar transações automaticamente

---

## Parte 2 — Nova Estrutura de Projetos

### 2.1 Estrutura Alvo

```
Work-IA.sln
├── src/
│   ├── Work-IA.Domain/              → Entidades, VOs, Eventos, Ports
│   ├── Work-IA.Application/         → Commands, Queries, Handlers, Behaviors
│   ├── Work-IA.Infrastructure/      → EF Core, EventBus, Adapters, Services
│   ├── Work-IA.Api/                 → Controllers, Hubs, Middleware (renomeado de WebApi)
│   ├── Work-IA.Office/              → Silk.NET 3D Office (movido de Presentation/)
│   ├── Work-IA.AppHost/             → Aspire (movido de Aspire/)
│   └── Work-IA.Dashboard/           → Blazor WASM (renomeado de BlazorDashboard)
├── tests/                           → Testes
│   ├── Work-IA.Domain.Tests/
│   ├── Work-IA.Application.Tests/
│   └── Work-IA.Infrastructure.Tests/
└── assets/                          → 3D models, texturas
```

**Remover:**
- `Work-IA.ServiceDefaults` — mover Extensions.cs para Application
- `Work-IA.CLI` — remover projeto inútil

**Renomear:**
- `Work-IA.WebApi` → `Work-IA.Api`
- `Work-IA.BlazorDashboard` → `Work-IA.Dashboard`

**Mover:**
- `Work-IA.Office` de `Presentation/` → `src/` (raiz)
- `Work-IA.AppHost` de `Aspire/` → `src/` (raiz)

---

## Parte 3 — Refatoração Prévia (Sprint R0)

### 3.1 Mover RoleDefinition para Agents/
Renomear namespace `Work_IA.Domain.Roles` → `Work_IA.Domain.Agents`

### 3.2 Extrair Mappers dos Repositories
Criar `Infrastructure/Persistence/Mappers/`:
- `AgentMapper.cs` — mapeia AgentEntity ↔ Agent
- `MemoryEntryMapper.cs` — mapeia MemoryEntryEntity ↔ MemoryEntry
- `WorkflowMapper.cs` — mapeia WorkflowInstanceEntity ↔ WorkflowInstance

### 3.3 Simplificar CreateAgentCommandHandler
Separar responsabilidades:
- Handler persiste apenas
- Domain Event `AgentCreatedDomainEvent` → Handler registra no runtime

### 3.4 Consolidar ServiceDefaults
Mover `Extensions.cs` para `Application/DependencyInjection.cs`.
Remover projeto `Work-IA.ServiceDefaults` da solução.

### 3.5 Extrair WorkflowEngine
Separar em:
- `WorkflowRegistry.cs` — gerencia definições
- `WorkflowExecutor.cs` — executa instâncias
- `WorkflowDefaults.cs` — registra workflows padrão

### 3.6 Extrair OfficeRenderer
Separar em:
- `Rendering/SceneRenderer.cs` — cena principal
- `Rendering/AgentRenderer.cs` — renderiza agentes
- `Rendering/OfficeGeometry.cs` — geometria do escritório
- `Rendering/CameraController.cs` — controle de câmera

---

## Parte 4 — Sprints Detalhadas

### Sprint R0 — Refatoração Estrutural (3 dias)

**Objetivo:** Limpar a casa antes de adicionar funcionalidade nova
**Commit:** `sprint-r0: refactor project structure and extract god classes`

| Task | Subtasks | Esforço |
|------|----------|---------|
| **R0.1** Renomear projetos | Renomear WebApi→Api, BlazorDashboard→Dashboard, mover Office/AppHost p/ src/ | 2h |
| **R0.2** Remover ServiceDefaults | Mover Extensions.cs p/ Application. Remover projeto da solução | 1h |
| **R0.3** Remover CLI | Remover projeto inútil | 30min |
| **R0.4** Mover RoleDefinition | `Domain/Roles/` → `Domain/Agents/`. Atualizar todos os usings | 1h |
| **R0.5** Extrair Mappers | Criar AgentMapper, MemoryMapper, WorkflowMapper. Extrair dos repositories | 3h |
| **R0.6** Separar CreateAgentHandler | Handler persiste. Domain Event registra runtime | 2h |
| **R0.7** Extrair WorkflowEngine | Separar em Registry + Executor + Defaults (3 classes) | 2h |
| **R0.8** Extrair OfficeRenderer | Separar em 4 classes: Scene, Agents, Geometry, Camera | 3h |
| **R0.9** Limpar .sln | Remover projetos mortos, reordenar estrutura | 1h |
| **R0.10** Build + Testes | Build limpo, 45+ testes, commit | 1h |

---

### Sprint R1 — Behavior Engine (5 dias)

**Objetivo:** Agentes ganham vida — trabalham, conversam, descansam
**Commit:** `sprint-r1: add agent behavior engine with autonomous actions`

| Task | Subtasks | Esforço |
|------|----------|---------|
| **R1.1** AgentAction enum | `Domain/Agents/AgentAction.cs` — Working, Chatting, Meeting, Resting, Idle | 30min |
| **R1.2** AgentBehaviorState | `Application/Agents/AgentBehaviorState.cs` — estado atual + timer + histórico | 1h |
| **R1.3** BehaviorDecider | Lógica de decisão: o que fazer baseado em estado + personalidade + contexto | 3h |
| **R1.4** ConversationSimulator | Simula conversas entre agentes com tópicos + turnos + emoção resultante | 3h |
| **R1.5** WorkSimulator | Gera tarefas automaticamente, agentes pegam da fila | 2h |
| **R1.6** MeetingScheduler | Agenda reuniões quando múltiplos agentes estão livres | 2h |
| **R1.7** RestSimulator | Agentes fazem pausa programada (ociosidade) | 1h |
| **R1.8** SignalR Broadcast | Broadcasting de TODAS as ações em tempo real | 2h |
| **R1.9** Testes | Testar decider + simulator + scheduler | 2h |
| **R1.10** Build + Commit | | 1h |

---

### Sprint R2 — Task Board e Fila (3 dias)

**Objetivo:** Kanban visual de tarefas
**Commit:** `sprint-r2: implement task board with kanban and work queue`

| Task | Subtasks | Esforço |
|------|----------|---------|
| **R2.1** TaskBoard aggregate | Domain — colunas + cartões + movimentação | 2h |
| **R2.2** TaskBoard API | Commands: CriarTarefa, MoverTarefa, ConcluirTarefa; Queries: ListarBoard | 3h |
| **R2.3** WorkQueue domain | Fila de prioridade com Deadlines | 1h |
| **R2.4** WorkQueue API | Enfileirar, processar, priorizar | 2h |
| **R2.5** Dashboard Board page | Página /board com arrastar colunas | 3h |
| **R2.6** Dashboard Fila page | Página /fila com prioridade | 2h |
| **R2.7** Testes | Testar board + fila | 2h |
| **R2.8** Build + Commit | | 1h |

---

### Sprint R3 — Reuniões e Conversas (3 dias)

**Objetivo:** Sistema de reuniões visível no frontend
**Commit:** `sprint-r3: implement meeting system with chat and calendar`

| Task | Subtasks | Esforço |
|------|----------|---------|
| **R3.1** Meeting aggregate | Domain — pauta, participantes, decisões, status | 2h |
| **R3.2** Meeting API | Commands: Agendar, Iniciar, Finalizar; Queries: Listar, Obter | 2h |
| **R3.3** Realtime chat | SignalR para conversas em reunião | 2h |
| **R3.4** Dashboard Reunioes | Página /reunioes com calendário | 2h |
| **R3.5** Dashboard Sala | Página /reuniao/{id} com chat + pauta + decisões | 3h |
| **R3.6** Notificações | Toast quando reunião começa | 1h |
| **R3.7** Testes | Testar meetings + chat | 2h |
| **R3.8** Build + Commit | | 1h |

---

### Sprint R4 — Dashboard Funcional (4 dias)

**Objetivo:** Dashboard mostrando dados reais com gráficos
**Commit:** `sprint-r4: implement functional dashboard with real-time data and charts`

| Task | Subtasks | Esforço |
|------|----------|---------|
| **R4.1** DashboardSummary query | Query consolidada com métricas de todos os agentes | 2h |
| **R4.2** DashboardSignalR hub | Streaming de métricas atualizadas | 2h |
| **R4.3** Cards de métricas | Funcionários, Online, XP Total, Custo, Tarefas Hoje (dados REAIS) | 2h |
| **R4.4** Gráfico de atividade | Chart.js/Blorc — atividade dos agentes por hora | 3h |
| **R4.5** Gráfico de custo | Custo por agente por dia/semana/mês | 2h |
| **R4.6** Lista de agentes real | Tabela com dados reais da API, não mockados | 2h |
| **R4.7** Hiring form funcional | Contratar → POST /api/agents → atualiza lista | 1h |
| **R4.8** Testes | Testar queries + hubs + componentes | 2h |
| **R4.9** Build + Commit | | 1h |

---

### Sprint R5 — Dinâmica Social (5 dias)

**Objetivo:** Amizade, demissão, competição, analytics
**Commit:** `sprint-r5: implement social dynamics with friendship, firing, and competition`

| Task | Subtasks | Esforço |
|------|----------|---------|
| **R5.1** Friendship entity | Domain — AgentA, AgentB, AffinityLevel, Interactions | 1h |
| **R5.2** Friendship service | Lógica: interagir → +affinity, conflito → -affinity | 2h |
| **R5.3** Friendship effects | Amigos colaboram 20% mais rápido. Inimigos pedem separação | 2h |
| **R5.4** Fire system | Comando FireAgent com consequências sociais | 2h |
| **R5.5** Performance review | Review automática a cada N tarefas | 2h |
| **R5.6** Competition ranking | Score composto: produção × qualidade ÷ custo | 1h |
| **R5.7** Promotion XP bonus | Agente com maior eficiência ganha bônus | 1h |
| **R5.8** Analytics dashboard | Página /analytics com quem mais entrega e quem gasta menos | 3h |
| **R5.9** Friends graph page | Página /amizades com grafo visual | 2h |
| **R5.10** Firing history page | Página /demissoes com histórico | 1h |
| **R5.11** Testes | Testar friendships + firing + review | 2h |
| **R5.12** Build + Commit | | 1h |

---

### Sprint R6 — Office 3D Vivo (4 dias)

**Objetivo:** Escritório 3D com agentes reais animados
**Commit:** `sprint-r6: animate 3d office with real agent interactions`

| Task | Subtasks | Esforço |
|------|----------|---------|
| **R6.1** SignalR real-time agent sync | Office conecta no backend e recebe posições/emoções reais | 2h |
| **R6.2** Agent 3D movement | Agentes andam smooth entre posições | 2h |
| **R6.3** Conversation bubbles | Bolhas "💬 discutindo arquitetura" acima dos agentes | 2h |
| **R6.4** Emotion indicators | 😊😰🎉🤔 flutuando sobre cada agente | 1h |
| **R6.5** Meeting room highlight | Sala destaca quando agentes estão em reunião | 1h |
| **R6.6** Click agent to follow | Câmera segue agente clicado | 2h |
| **R6.7** Agent name labels | Nome + cargo visível sobre cada agente | 1h |
| **R6.8** Idle animations | Agentes ociosos mexem levemente | 2h |
| **R6.9** Testes | Testar sincronização + animações | 2h |
| **R6.10** Build + Commit | | 1h |

---

### Sprint R7 — Integração Final (3 dias)

**Objetivo:** Tudo funcionando com F5 pelo Aspire
**Commit:** `sprint-r7: final integration and polish`

| Task | Subtasks | Esforço |
|------|----------|---------|
| **R7.1** Aspire final config | AppHost orquestra Api + Dashboard + Redis + RabbitMQ | 2h |
| **R7.2** Seed inicial | Criar 3 agentes + 3 cargos automaticamente | 1h |
| **R7.3** Fluxo completo E2E | Cargo → Agente → Trabalha → Conversa → Reúne → Promove → Demite | 3h |
| **R7.4** Remover todo mock | Substituir TODOS os dados mockados por chamadas reais | 2h |
| **R7.5** Limpar código morto | Remover arquivos não utilizados, using desnecessários | 2h |
| **R7.6** Remover comentários | Limpar qualquer comentário restante | 1h |
| **R7.7** Testes finais | 45+ testes passando, cobertura mínima | 2h |
| **R7.8** Documentação | README atualizado com novo fluxo | 1h |
| **R7.9** Release tag | v2.0.0 — "AI Office OS - Live Office" | 30min |

---

## Cronograma Geral

```
Sprint R0 ████████████████░░░░░░░░░░░░░░░░░░░░░░░░  3 dias  Refatoração
Sprint R1 ░░░░░░░░████████████████████░░░░░░░░░░░░  5 dias  Behavior Engine
Sprint R2 ░░░░░░░░░░░░░░░░░░██████████████░░░░░░░░  3 dias  Task Board
Sprint R3 ░░░░░░░░░░░░░░░░░░░░░░░░████████████░░░░  3 dias  Reuniões
Sprint R4 ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░████████░░  4 dias  Dashboard
Sprint R5 ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░████  5 dias  Social Dynamics
Sprint R6 ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  4 dias  Office 3D Vivo
Sprint R7 ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  3 dias  Integração
         └────────────── 30 dias ────────────────┘
```

## Regras para Cada Sprint

1. **1 commit por sprint** — mensagem `sprint-r{N}: {título}`
2. **Build 0 erros obrigatório** antes de commitar
3. **45+ testes passando** antes de commitar
4. **Sem comentários ou summaries** no código
5. **Pipeline**: Especialistas implementam → Revisor aprova → Builder compila
6. **Cada entrega**: código limpo + build verde + testes + push

---

## Conclusão

O projeto atual tem 7.560 linhas em 310 arquivos, mas **falta o comportamento dos agentes**. Este plano refatora a estrutura inchada (Sprint R0) e constrói funcionalidade real (Sprints R1-R7) em 30 dias.

**Do jeito que está:** Esqueleto bonito, sem alma.
**Do jeito que vai ficar:** Escritório vivo com agentes que trabalham, conversam, fazem amizades, competem e evoluem.
