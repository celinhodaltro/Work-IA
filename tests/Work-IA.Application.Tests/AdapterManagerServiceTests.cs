using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Application.Services;
using Work_IA.Domain.Workspace.Ports;

namespace Work_IA.Application.Tests;

public sealed class AdapterManagerServiceTests
{
    private readonly IPluginLoader _loader;
    private readonly ILogger<AdapterManagerService> _logger;
    private readonly AdapterManagerService _service;

    public AdapterManagerServiceTests()
    {
        _loader = Substitute.For<IPluginLoader>();
        _logger = Substitute.For<ILogger<AdapterManagerService>>();
        _service = new AdapterManagerService(_loader, _logger);
    }

    [Fact]
    public void GetAdapter_WithUnconfiguredPlatform_ShouldReturnNull()
    {
        var result = _service.GetAdapter("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public void GetPrimaryAdapter_WhenNoAdapters_ShouldThrowInvalidOperationException()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => _service.GetPrimaryAdapter());

        Assert.Equal("No active adapters", exception.Message);
    }
}
