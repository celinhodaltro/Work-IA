using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Workspace;
using Work_IA.Domain.Workspace.Ports;
using Work_IA.Infrastructure.Adapters;
using Work_IA.Infrastructure.BackgroundServices;
using Work_IA.Infrastructure.Communication;
using Work_IA.Infrastructure.EventBus;
using Work_IA.Infrastructure.Persistence;
using Work_IA.Infrastructure.Persistence.EventStore;
using Work_IA.Infrastructure.Persistence.Interceptors;
using Work_IA.Infrastructure.Services;
using Work_IA.Infrastructure.Workflows;
using Work_IA.Infrastructure.Workspace;

namespace Work_IA.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var workspacePath = configuration.GetValue<string>("Workspace:Path")
            ?? Directory.GetCurrentDirectory();

        var ignorePatterns = configuration.GetSection("Workspace:IgnorePatterns").Get<List<string>>();

        services.AddScoped<DispatchDomainEventsInterceptor>();

        services.AddDbContext<WorkIaDbContext>((sp, options) =>
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(WorkIaDbContext).Assembly.FullName))
                .AddInterceptors(sp.GetRequiredService<DispatchDomainEventsInterceptor>()));

        services.AddSingleton<IEventTypeResolver>(sp =>
        {
            var domainAssembly = typeof(Work_IA.Domain.Abstractions.IDomainEvent).Assembly;
            var types = domainAssembly.GetTypes()
                .Where(t => t.IsAssignableTo(typeof(IDomainEvent)) && !t.IsAbstract && !t.IsInterface)
                .ToList();
            return new EventTypeResolver(types);
        });

        services.AddScoped<IEventStore, EventStore>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        services.AddSingleton<ICommunicationBus, CommunicationBus>();
        services.AddSingleton<IFileSystemService>(new FileSystemService(workspacePath));
        services.AddSingleton<IGitIntegrationService>(new GitIntegrationService(workspacePath));
        services.AddSingleton<IFileSystemObserver>(sp =>
        {
            var observer = new FileSystemWatcherObserver(ignorePatterns);
            var workspacePathObj = new WorkspacePath(workspacePath);
            observer.StartAsync(workspacePathObj).GetAwaiter().GetResult();
            return observer;
        });

        // OpenCode Adapter registration
        services.AddSingleton<OpenCodeAdapter>();
        services.AddSingleton<IOpenCodeOperations>(sp => sp.GetRequiredService<OpenCodeAdapter>());
        services.AddSingleton<IWorkspaceAdapter>(sp => sp.GetRequiredService<OpenCodeAdapter>());
        services.AddSingleton<FileSystemWorkspaceAdapter>();
        services.AddSingleton<WorkspaceAdapterFactory>();
        services.AddHostedService<OpenCodeBackgroundService>();

        // Plugin system registration
        services.AddSingleton<IPluginLoader>(sp =>
        {
            var loader = new PluginLoader(sp, sp.GetRequiredService<ILogger<PluginLoader>>());
            loader.RegisterPlugin(new ClaudeCodePlugin());
            return loader;
        });

        services.AddSingleton<AgentStartupService>();
        services.AddSingleton<IAgentInitializationService>(sp => sp.GetRequiredService<AgentStartupService>());
        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<AgentStartupService>());

        services.AddSingleton<IWorkflowEngine, WorkflowEngine>();
        services.AddHostedService<WorkflowStartupService>();

        return services;
    }
}
