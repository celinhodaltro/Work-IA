using Xunit;
using Work_IA.Domain.Adapters;

namespace Work_IA.Domain.Tests;

public sealed class AdapterConfigurationTests
{
    [Fact]
    public void DefaultValues_ShouldBeFilesystemPlatform()
    {
        var config = new AdapterConfiguration();

        Assert.Equal("filesystem", config.Platform);
    }

    [Fact]
    public void DefaultValues_ShouldBeEnabled()
    {
        var config = new AdapterConfiguration();

        Assert.True(config.Enabled);
    }

    [Fact]
    public void DefaultValues_ShouldHavePriority10()
    {
        var config = new AdapterConfiguration();

        Assert.Equal(10, config.Priority);
    }

    [Fact]
    public void Settings_ShouldBeEmptyByDefault()
    {
        var config = new AdapterConfiguration();

        Assert.Empty(config.Settings);
    }

    [Fact]
    public void Settings_ShouldStoreAndRetrieveValues()
    {
        var config = new AdapterConfiguration();
        config.Settings["key1"] = "value1";
        config.Settings["key2"] = "value2";

        Assert.Equal("value1", config.Settings["key1"]);
        Assert.Equal("value2", config.Settings["key2"]);
        Assert.Equal(2, config.Settings.Count);
    }

    [Fact]
    public void Settings_ShouldAcceptInitialDictionary()
    {
        var settings = new Dictionary<string, string>
        {
            ["endpoint"] = "http://localhost",
            ["timeout"] = "30"
        };

        var config = new AdapterConfiguration
        {
            Platform = "custom",
            Enabled = false,
            Priority = 5,
            Settings = settings
        };

        Assert.Equal("custom", config.Platform);
        Assert.False(config.Enabled);
        Assert.Equal(5, config.Priority);
        Assert.Equal(2, config.Settings.Count);
        Assert.Equal("http://localhost", config.Settings["endpoint"]);
        Assert.Equal("30", config.Settings["timeout"]);
    }
}
