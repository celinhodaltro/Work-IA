using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Infrastructure.Adapters;

namespace Work_IA.Infrastructure.Tests;

public sealed class ClaudeCodeAdapterTests
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<ClaudeCodeAdapter> _logger;
    private readonly ClaudeCodeAdapter _adapter;

    public ClaudeCodeAdapterTests()
    {
        _eventBus = Substitute.For<IEventBus>();
        _logger = Substitute.For<ILogger<ClaudeCodeAdapter>>();
        _adapter = new ClaudeCodeAdapter(_eventBus, _logger);
    }

    [Fact]
    public void PlatformName_ShouldReturnClaudeCode()
    {
        Assert.Equal("Claude Code", _adapter.PlatformName);
    }

    [Fact]
    public async Task GetFilesAsync_ShouldReturnEmptyList()
    {
        var files = await _adapter.GetFilesAsync();

        Assert.NotNull(files);
        Assert.Empty(files);
    }

    [Fact]
    public async Task ReadFileAsync_ShouldReturnNull()
    {
        var result = await _adapter.ReadFileAsync("test.txt");

        Assert.Null(result);
    }

    [Fact]
    public async Task WriteFileAsync_ShouldNotThrow()
    {
        var exception = await Record.ExceptionAsync(() =>
            _adapter.WriteFileAsync("test.txt", "content"));

        Assert.Null(exception);
    }

    [Fact]
    public async Task DeleteFileAsync_ShouldNotThrow()
    {
        var exception = await Record.ExceptionAsync(() =>
            _adapter.DeleteFileAsync("test.txt"));

        Assert.Null(exception);
    }

    [Fact]
    public async Task IsConnectedAsync_Initially_ShouldReturnFalse()
    {
        var connected = await _adapter.IsConnectedAsync();

        Assert.False(connected);
    }

    [Fact]
    public async Task ConnectAsync_ShouldEstablishConnection()
    {
        await _adapter.ConnectAsync("http://localhost");

        var connected = await _adapter.IsConnectedAsync();
        Assert.True(connected);
    }

    [Fact]
    public async Task DisconnectAsync_ShouldDisconnect()
    {
        await _adapter.ConnectAsync("http://localhost");
        await _adapter.DisconnectAsync();

        var connected = await _adapter.IsConnectedAsync();
        Assert.False(connected);
    }
}
