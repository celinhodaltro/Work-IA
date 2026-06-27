using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Workspace.Ports;
using Work_IA.Infrastructure.Adapters;

namespace Work_IA.Infrastructure.Tests;

public sealed class ClaudeCodePluginTests
{
    [Fact]
    public void Name_ShouldReturnClaudeCodeAdapter()
    {
        var plugin = new ClaudeCodePlugin();

        Assert.Equal("Claude Code Adapter", plugin.Name);
    }

    [Fact]
    public void Version_ShouldReturn1_0_0()
    {
        var plugin = new ClaudeCodePlugin();

        Assert.Equal("1.0.0", plugin.Version);
    }

    [Fact]
    public void SupportedPlatform_ShouldReturnClaudeCode()
    {
        var plugin = new ClaudeCodePlugin();

        Assert.Equal("claude-code", plugin.SupportedPlatform);
    }

    [Fact]
    public async Task CreateAdapterAsync_ShouldReturnClaudeCodeAdapter()
    {
        var eventBus = Substitute.For<IEventBus>();
        var logger = Substitute.For<ILogger<ClaudeCodeAdapter>>();
        var services = Substitute.For<IServiceProvider>();

        services.GetService(typeof(IEventBus)).Returns(eventBus);
        services.GetService(typeof(ILogger<ClaudeCodeAdapter>)).Returns(logger);

        var plugin = new ClaudeCodePlugin();

        var adapter = await plugin.CreateAdapterAsync(services);

        Assert.NotNull(adapter);
        Assert.IsType<ClaudeCodeAdapter>(adapter);
        Assert.Equal("Claude Code", adapter.PlatformName);
    }
}
