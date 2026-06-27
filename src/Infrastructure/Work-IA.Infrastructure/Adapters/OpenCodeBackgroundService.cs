using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Work_IA.Infrastructure.Adapters;

public sealed class OpenCodeBackgroundService : BackgroundService
{
    private readonly IOpenCodeOperations _adapter;
    private readonly ILogger<OpenCodeBackgroundService> _logger;

    public OpenCodeBackgroundService(IOpenCodeOperations adapter, ILogger<OpenCodeBackgroundService> logger)
    {
        _adapter = adapter;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OpenCode background service starting");

        await _adapter.ConnectAsync("opencode://local", stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            if (!await _adapter.IsConnectedAsync(stoppingToken))
            {
                _logger.LogWarning("OpenCode adapter disconnected, reconnecting...");
                await _adapter.ConnectAsync("opencode://local", stoppingToken);
            }
        }
    }
}
