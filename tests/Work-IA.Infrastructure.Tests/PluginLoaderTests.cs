using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Workspace.Ports;
using Work_IA.Infrastructure.Adapters;

namespace Work_IA.Infrastructure.Tests;

public sealed class PluginLoaderTests
{
    private static PluginLoader CreateLoader()
    {
        var services = Substitute.For<IServiceProvider>();
        var logger = Substitute.For<ILogger<PluginLoader>>();
        return new PluginLoader(services, logger);
    }

    private static IAdapterPlugin CreateMockPlugin(string platform, string name = "Test Plugin", string version = "1.0.0")
    {
        var plugin = Substitute.For<IAdapterPlugin>();
        plugin.Name.Returns(name);
        plugin.Version.Returns(version);
        plugin.SupportedPlatform.Returns(platform);
        return plugin;
    }

    [Fact]
    public void RegisterPlugin_WithValidPlugin_ShouldAddToPlugins()
    {
        var loader = CreateLoader();
        var plugin = CreateMockPlugin("test-platform");

        loader.RegisterPlugin(plugin);

        Assert.Single(loader.Plugins);
        Assert.Same(plugin, loader.Plugins[0]);
    }

    [Fact]
    public void RegisterPlugin_WithDuplicatePlatform_ShouldNotAdd()
    {
        var loader = CreateLoader();
        var plugin1 = CreateMockPlugin("test-platform", "Plugin A");
        var plugin2 = CreateMockPlugin("test-platform", "Plugin B");

        loader.RegisterPlugin(plugin1);
        loader.RegisterPlugin(plugin2);

        Assert.Single(loader.Plugins);
        Assert.Same(plugin1, loader.Plugins[0]);
    }

    [Fact]
    public async Task CreateAdapterAsync_WithRegisteredPlatform_ShouldReturnAdapter()
    {
        var services = Substitute.For<IServiceProvider>();
        var logger = Substitute.For<ILogger<PluginLoader>>();
        var loader = new PluginLoader(services, logger);

        var adapter = Substitute.For<IWorkspaceAdapter>();
        var plugin = Substitute.For<IAdapterPlugin>();
        plugin.SupportedPlatform.Returns("test-platform");
        plugin.CreateAdapterAsync(services, default).Returns(adapter);

        loader.RegisterPlugin(plugin);

        var result = await loader.CreateAdapterAsync("test-platform");

        Assert.NotNull(result);
        Assert.Same(adapter, result);
    }

    [Fact]
    public async Task CreateAdapterAsync_WithUnregisteredPlatform_ShouldReturnNull()
    {
        var loader = CreateLoader();

        var result = await loader.CreateAdapterAsync("unknown-platform");

        Assert.Null(result);
    }
}
