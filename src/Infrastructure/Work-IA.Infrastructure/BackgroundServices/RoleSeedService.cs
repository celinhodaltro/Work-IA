using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Infrastructure.BackgroundServices;

public sealed class RoleSeedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RoleSeedService> _logger;

    public RoleSeedService(IServiceScopeFactory scopeFactory, ILogger<RoleSeedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRoleRepository>();

        var existing = await repo.GetAllAsync(cancellationToken);
        if (existing.Count > 0)
        {
            _logger.LogInformation("Roles already seeded ({Count} roles)", existing.Count);
            return;
        }

        var roles = GetSeedRoles();
        foreach (var role in roles)
        {
            await repo.AddAsync(role, cancellationToken);
        }

        _logger.LogInformation("Seeded {Count} roles successfully", roles.Count);

        await SeedHeadAgentAsync(scope, cancellationToken);
    }

    private async Task SeedHeadAgentAsync(IServiceScope scope, CancellationToken ct)
    {
        var agentRepo = scope.ServiceProvider.GetRequiredService<IAgentRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var existing = await agentRepo.GetAllAsync(ct);
        if (existing.Any(a => a.IsHead))
        {
            _logger.LogInformation("Head agent already exists");
            return;
        }

        var headAgent = Agent.Create(
            new AgentName("Head"),
            new AgentTitle("Head"),
            null,
            new AgentPermissions(false, false, true, null),
            true);

        await agentRepo.AddAsync(headAgent, ct);
        await unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Head agent created");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static List<RoleDefinition> GetSeedRoles()
    {
        return
        [
            RoleDefinition.Hydrate("Head of Engineering", 
            [
                "System Architecture", "Leadership", "Strategy"
            ],
            [
                "Agile", "SAFe", "OKR"
            ],
            [
                "OpenCode", "GitHub", "Azure DevOps", "Jira"
            ]),

            RoleDefinition.Hydrate("Tech Lead Backend",
            [
                "C#", ".NET", "ASP.NET Core", "Entity Framework", "Dapper", "SQL", "PostgreSQL", "Redis", "RabbitMQ", "MassTransit"
            ],
            [
                "Clean Architecture", "DDD", "CQRS", "SOLID", "Clean Code", "Event Driven Architecture", "Saga Pattern", "Outbox Pattern"
            ],
            [
                "Visual Studio", "Rider", "GitHub", "Docker", "Kubernetes", "Seq", "Grafana", "Prometheus"
            ]),

            RoleDefinition.Hydrate("Tech Lead Frontend",
            [
                "C#", "Blazor", "WASM", "SSR", "MudBlazor", "Radzen", "CSS", "JavaScript", "TypeScript"
            ],
            [
                "Component Design", "Responsive Design", "UX", "Acessibilidade", "Performance Web"
            ],
            [
                "Visual Studio", "Chrome DevTools", "Lighthouse", "GitHub"
            ]),

            RoleDefinition.Hydrate("Tech Lead Game",
            [
                "C#", "Silk.NET", "OpenGL", "GLSL", "ECS", "3D Math", "Animation"
            ],
            [
                "Game Architecture", "Asset Pipeline", "Multiplayer"
            ],
            [
                "Visual Studio", "RenderDoc", "Blender", "GitHub"
            ]),

            RoleDefinition.Hydrate("Tech Lead DevOps",
            [
                "Docker", "Kubernetes", "YAML", "Bash", "PowerShell", "Terraform"
            ],
            [
                "CI/CD", "GitOps", "Infrastructure as Code", "SRE", "Observabilidade"
            ],
            [
                "GitHub Actions", "Azure DevOps", "Prometheus", "Grafana", "Seq", "OpenTelemetry", "Datadog"
            ]),

            RoleDefinition.Hydrate("Clean Architecture Specialist",
            [
                "C#", ".NET", "ASP.NET Core"
            ],
            [
                "Clean Architecture", "DDD", "CQRS", "SOLID", "Clean Code", "Vertical Slice Architecture", "Dependency Injection"
            ],
            [
                "Visual Studio", "Rider", "NDepend"
            ]),

            RoleDefinition.Hydrate("DDD Specialist",
            [
                "C#", ".NET"
            ],
            [
                "Domain Driven Design", "Bounded Contexts", "Aggregates", "Value Objects", "Domain Events", "Entity", "Repository Pattern"
            ],
            [
                "Visual Studio", "EventStorming Tools"
            ]),

            RoleDefinition.Hydrate("CQRS Specialist",
            [
                "C#", ".NET", "ASP.NET Core", "MediatR", "MassTransit"
            ],
            [
                "CQRS", "Command Query Separation", "Event Sourcing", "Task Based UI"
            ],
            [
                "Visual Studio", "Swagger", "Postman"
            ]),

            RoleDefinition.Hydrate("Microservices Specialist",
            [
                "C#", ".NET", "ASP.NET Core", "Docker", "Kubernetes", "RabbitMQ", "MassTransit", "Redis", "PostgreSQL"
            ],
            [
                "Microservices", "Event Driven Architecture", "Saga Pattern", "Outbox Pattern", "IdempotÃªncia", "Circuit Breaker", "Retry Policy"
            ],
            [
                "Docker", "Kubernetes", "Seq", "Grafana", "Prometheus", "OpenTelemetry", "Azure Service Bus"
            ]),

            RoleDefinition.Hydrate("Entity Framework Specialist",
            [
                "C#", ".NET", "Entity Framework Core", "SQL", "PostgreSQL", "SQL Server"
            ],
            [
                "Code First", "Migrations", "Performance SQL", "Ãndices", "N+1 Query Prevention"
            ],
            [
                "Visual Studio", "SSMS", "PgAdmin", "Azure Data Studio"
            ]),

            RoleDefinition.Hydrate("SQL Specialist",
            [
                "SQL", "T-SQL", "PL/pgSQL", "PostgreSQL", "SQL Server"
            ],
            [
                "Query Optimization", "Ãndices", "Stored Procedures", "CTE", "Window Functions"
            ],
            [
                "SSMS", "PgAdmin", "DBeaver", "Azure Data Studio"
            ]),

            RoleDefinition.Hydrate("Blazor Specialist",
            [
                "C#", ".NET", "Blazor", "WASM", "SSR", "MudBlazor", "Radzen", "CSS"
            ],
            [
                "Component Design", "State Management", "Render Tree", "Performance WASM"
            ],
            [
                "Visual Studio", "Chrome DevTools", "Lighthouse"
            ]),

            RoleDefinition.Hydrate("Silk.NET Game Specialist",
            [
                "C#", "Silk.NET", "OpenGL", "GLSL", "Assimp", "3D Math"
            ],
            [
                "Game Loop", "ECS", "Rendering Pipeline", "Shader Programming", "Asset Pipeline"
            ],
            [
                "Visual Studio", "RenderDoc", "Blender", "ShaderToy"
            ]),

            RoleDefinition.Hydrate("OpenGL Specialist",
            [
                "C#", "OpenGL", "GLSL", "Silk.NET", "3D Math"
            ],
            [
                "Shader Programming", "Frame Buffer", "Tessellation", "Compute Shaders"
            ],
            [
                "RenderDoc", "ShaderToy", "NVIDIA Nsight"
            ]),

            RoleDefinition.Hydrate("Architecture Specialist",
            [
                "C#", ".NET", "ASP.NET Core", "Docker", "Kubernetes"
            ],
            [
                "Clean Architecture", "DDD", "CQRS", "SOLID", "Microservices", "Event Driven Architecture"
            ],
            [
                "Visual Studio", "Draw.io", "Miro", "Notion"
            ]),

            RoleDefinition.Hydrate("Performance Specialist",
            [
                "C#", ".NET", "ASP.NET Core", "SQL", "Redis"
            ],
            [
                "Benchmarking", "Profiling", "Memory Analysis", "GC Tuning", "Async Performance"
            ],
            [
                "BenchmarkDotNet", "dotTrace", "PerfView", "Visual Studio Diagnostic Tools"
            ]),

            RoleDefinition.Hydrate("Security Specialist",
            [
                "C#", ".NET", "ASP.NET Core", "SQL"
            ],
            [
                "OWASP", "Security by Design", "Authentication", "Authorization", "Cryptography"
            ],
            [
                "SonarQube", "OWASP ZAP", "Snyk", "GitHub Security"
            ]),

            RoleDefinition.Hydrate("Git Specialist",
            [
                "Git", "GitHub", "GitFlow", "GitHub Actions"
            ],
            [
                "Version Control", "Code Review", "Branch Strategy", "Merge Conflict Resolution"
            ],
            [
                "GitHub", "Git CLI", "SourceTree", "GitKraken"
            ]),

            RoleDefinition.Hydrate("DevOps Specialist",
            [
                "Docker", "Kubernetes", "YAML", "Bash", "PowerShell", "Terraform"
            ],
            [
                "CI/CD", "Infrastructure as Code", "GitOps", "SRE"
            ],
            [
                "GitHub Actions", "Azure DevOps", "Jenkins", "Helm", "ArgoCD"
            ]),

            RoleDefinition.Hydrate("Reviewer",
            [
                "C#", ".NET", "ASP.NET Core", "Blazor", "Silk.NET"
            ],
            [
                "Clean Architecture", "DDD", "CQRS", "SOLID", "Clean Code", "Code Review", "Security Review"
            ],
            [
                "GitHub", "SonarQube", "NDepend"
            ]),

            RoleDefinition.Hydrate("Tester",
            [
                "C#", ".NET", "SQL"
            ],
            [
                "Testes UnitÃ¡rios", "Testes de IntegraÃ§Ã£o", "Testes E2E", "BDD", "TDD", "Cobertura de CÃ³digo"
            ],
            [
                "xUnit", "NUnit", "Moq", "FluentAssertions", "Playwright", "Selenium", "Testcontainers"
            ]),

            RoleDefinition.Hydrate("Documentation Specialist",
            [
                "Markdown", "YAML", "JSON"
            ],
            [
                "DocumentaÃ§Ã£o TÃ©cnica", "ADR", "API Documentation", "Wiki"
            ],
            [
                "Swagger", "GitHub Wiki", "Notion", "Obsidian", "Docusaurus"
            ]),

            RoleDefinition.Hydrate("Debug Specialist",
            [
                "C#", ".NET", "ASP.NET Core", "SQL"
            ],
            [
                "Debugging", "Root Cause Analysis", "Post Mortem"
            ],
            [
                "Visual Studio", "Rider", "WinDbg", "dotMemory", "Seq"
            ]),

            RoleDefinition.Hydrate("Refactoring Specialist",
            [
                "C#", ".NET", "ASP.NET Core"
            ],
            [
                "Refactoring", "Code Smells", "Design Patterns", "SOLID", "Clean Code"
            ],
            [
                "Visual Studio", "Rider", "ReSharper", "SonarQube"
            ]),
        ];
    }
}
