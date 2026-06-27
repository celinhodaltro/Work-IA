using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Adapters;
using Work_IA.Domain.Workspace.Ports;

namespace Work_IA.Application.Services;

public sealed class AdapterManagerService
{
    private readonly IPluginLoader _loader;
    private readonly ILogger<AdapterManagerService> _logger;
    private readonly List<AdapterConfiguration> _configs = [];
    private readonly Dictionary<string, IWorkspaceAdapter> _activeAdapters = [];

    public IReadOnlyDictionary<string, IWorkspaceAdapter> ActiveAdapters => _activeAdapters;

    public AdapterManagerService(IPluginLoader loader, ILogger<AdapterManagerService> logger)
    {
        _loader = loader;
        _logger = logger;
    }

    public void Configure(string platform, bool enabled, int priority = 10, Dictionary<string, string>? settings = null)
    {
        var config = _configs.FirstOrDefault(c => c.Platform == platform);

        if (config is null)
        {
            config = new AdapterConfiguration
            {
                Platform = platform,
                Enabled = enabled,
                Priority = priority,
                Settings = settings ?? []
            };
            _configs.Add(config);
        }
        else
        {
            config.Enabled = enabled;
            config.Priority = priority;
            config.Settings = settings ?? config.Settings;
        }
    }

    public async Task InitializeAdaptersAsync(CancellationToken cancellationToken = default)
    {
        var orderedConfigs = _configs
            .Where(c => c.Enabled)
            .OrderBy(c => c.Priority)
            .ToList();

        foreach (var config in orderedConfigs)
        {
            try
            {
                var adapter = await _loader.CreateAdapterAsync(config.Platform, cancellationToken);

                if (adapter is not null)
                {
                    _activeAdapters[config.Platform] = adapter;
                    _logger.LogInformation("Adapter activated: {Platform}", config.Platform);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to activate adapter for {Platform}", config.Platform);
            }
        }
    }

    public IWorkspaceAdapter? GetAdapter(string platform)
    {
        _activeAdapters.TryGetValue(platform, out var adapter);
        return adapter;
    }

    public IWorkspaceAdapter GetPrimaryAdapter()
    {
        return _activeAdapters.Values.FirstOrDefault()
            ?? throw new InvalidOperationException("No active adapters");
    }
}
