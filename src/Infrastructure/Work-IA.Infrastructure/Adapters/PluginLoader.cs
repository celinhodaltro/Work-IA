using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Workspace.Ports;

namespace Work_IA.Infrastructure.Adapters;

public sealed class PluginLoader : IPluginLoader
{
    private readonly IServiceProvider _services;
    private readonly ILogger<PluginLoader> _logger;
    private readonly List<IAdapterPlugin> _plugins = [];

    public IReadOnlyList<IAdapterPlugin> Plugins => _plugins.AsReadOnly();

    public PluginLoader(IServiceProvider services, ILogger<PluginLoader> logger)
    {
        _services = services;
        _logger = logger;
    }

    public void RegisterPlugin(IAdapterPlugin plugin)
    {
        if (_plugins.Any(p => p.SupportedPlatform == plugin.SupportedPlatform))
        {
            _logger.LogWarning("Plugin for {Platform} already registered", plugin.SupportedPlatform);
            return;
        }

        _plugins.Add(plugin);
        _logger.LogInformation("Plugin registered: {Name} v{Version}", plugin.Name, plugin.Version);
    }

    public async Task<IWorkspaceAdapter?> CreateAdapterAsync(string platform, CancellationToken cancellationToken = default)
    {
        var plugin = _plugins.FirstOrDefault(p => p.SupportedPlatform == platform);

        if (plugin is null)
        {
            _logger.LogWarning("No plugin found for platform: {Platform}", platform);
            return null;
        }

        try
        {
            return await plugin.CreateAdapterAsync(_services, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create adapter for {Platform}", platform);
            return null;
        }
    }

    public void LoadFromDirectory(string directory)
    {
        if (!Directory.Exists(directory))
        {
            _logger.LogWarning("Plugin directory not found: {Dir}", directory);
            return;
        }

        foreach (var dll in Directory.GetFiles(directory, "*.dll"))
        {
            try
            {
                var assembly = Assembly.LoadFrom(dll);
                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(IAdapterPlugin).IsAssignableFrom(t) && !t.IsAbstract);

                foreach (var type in pluginTypes)
                {
                    if (Activator.CreateInstance(type) is IAdapterPlugin plugin)
                    {
                        RegisterPlugin(plugin);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load plugin from {Dll}", dll);
            }
        }
    }
}
