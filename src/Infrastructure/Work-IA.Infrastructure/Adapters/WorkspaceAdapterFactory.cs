using Microsoft.Extensions.DependencyInjection;
using Work_IA.Domain.Workspace.Ports;

namespace Work_IA.Infrastructure.Adapters;

public sealed class WorkspaceAdapterFactory
{
    private readonly IServiceProvider _serviceProvider;

    public WorkspaceAdapterFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IWorkspaceAdapter Create(string platform)
    {
        return platform.ToLowerInvariant() switch
        {
            "opencode" => _serviceProvider.GetRequiredService<OpenCodeAdapter>(),
            "filesystem" => _serviceProvider.GetRequiredService<FileSystemWorkspaceAdapter>(),
            _ => _serviceProvider.GetRequiredService<FileSystemWorkspaceAdapter>()
        };
    }
}
