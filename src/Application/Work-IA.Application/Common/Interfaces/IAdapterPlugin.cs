using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Workspace.Ports;

namespace Work_IA.Application.Common.Interfaces;

public interface IAdapterPlugin
{
    string Name { get; }
    string Version { get; }
    string SupportedPlatform { get; }
    Task<IWorkspaceAdapter> CreateAdapterAsync(IServiceProvider services, CancellationToken cancellationToken = default);
}
