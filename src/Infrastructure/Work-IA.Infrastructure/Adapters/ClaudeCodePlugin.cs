using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Workspace.Ports;

namespace Work_IA.Infrastructure.Adapters;

public sealed class ClaudeCodePlugin : IAdapterPlugin
{
    public string Name => "Claude Code Adapter";
    public string Version => "1.0.0";
    public string SupportedPlatform => "claude-code";

    public Task<IWorkspaceAdapter> CreateAdapterAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var eventBus = services.GetRequiredService<IEventBus>();
        var logger = services.GetRequiredService<ILogger<ClaudeCodeAdapter>>();
        var adapter = new ClaudeCodeAdapter(eventBus, logger);
        return Task.FromResult<IWorkspaceAdapter>(adapter);
    }
}
