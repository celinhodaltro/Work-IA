using Work_IA.Domain.Workspace.Ports;

namespace Work_IA.Application.Common.Interfaces;

public interface IPluginLoader
{
    IReadOnlyList<IAdapterPlugin> Plugins { get; }
    void RegisterPlugin(IAdapterPlugin plugin);
    Task<IWorkspaceAdapter?> CreateAdapterAsync(string platform, CancellationToken cancellationToken = default);
    void LoadFromDirectory(string directory);
}
