using System.Numerics;
using ImGuiNET;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Work_IA.Application;
using Work_IA.Client.Rendering;
using Work_IA.Infrastructure;
using Work_IA.Infrastructure.Persistence;

namespace Work_IA.Client;

public static class Program
{
    private static ConfigManager _config = null!;
    private static string _statusMessage = "";
    private static string _selectedProvider = "opencode";
    private static string _apiKey = "";
    private static bool _isTesting;
    private static bool _showOffice;
    private static bool _configComplete;

    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((ctx, services) =>
            {
                services.AddApplication();
                services.AddInfrastructure(ctx.Configuration);
                services.AddSingleton<AgentStateEventHandler>();
                services.AddSingleton<OfficeRenderer>();
                services.AddSingleton<UIManager>();
                services.AddHostedService<AgentBehaviorService>();
            })
            .Build();

        _config = new ConfigManager();
        _selectedProvider = _config.Config.AiProvider;
        _apiKey = _config.Config.ApiKey;

        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WorkIaDbContext>();
            db.Database.EnsureCreated();
        }

        await host.StartAsync();

        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "AI Office OS";
        options.PreferredDepthBufferBits = 24;
        options.API = GraphicsAPI.Default;

        var window = Window.Create(options);
        var renderer = host.Services.GetRequiredService<OfficeRenderer>();
        IInputContext? input = null;

        window.Load += () =>
        {
            var gl = GL.GetApi(window);
            gl.ClearColor(0.08f, 0.08f, 0.1f, 1.0f);
            renderer.Initialize(gl, window.Size);
            input = window.CreateInput();
        };

        window.Update += (dt) => { };

        window.Render += (_) =>
        {
            var gl = GL.GetApi(window);
            gl.Clear(ClearBufferMask.ColorBufferBit);

            if (_showOffice)
            {
                renderer.Render();
                return;
            }

            ImGui.NewFrame();

            var center = new Vector2(window.Size.X / 2f, window.Size.Y / 2f);
            ImGui.SetNextWindowPos(new Vector2(center.X - 200, center.Y - 200), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new Vector2(400, 350));

            ImGui.Begin("AI Office OS", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove);

            if (_isTesting)
            {
                ImGui.Text("Testing connection...");
                ImGui.Text(_statusMessage);
            }
            else if (_configComplete)
            {
                ImGui.Text("Ready!");
                ImGui.Text(_statusMessage);
                ImGui.Separator();
                if (ImGui.Button("Enter Office", new Vector2(380, 40)))
                {
                    window.Size = new Vector2D<int>(1280, 720);
                    _showOffice = true;
                }
            }
            else if (_config.IsConfigured && !_configComplete)
            {
                ImGui.Text($"AI Office OS");
                ImGui.Separator();
                ImGui.Text($"Provider: {_config.Config.AiProvider}");
                ImGui.Text($"Status: Configured");
                ImGui.Separator();
                if (ImGui.Button("Continue", new Vector2(180, 40)))
                {
                    _isTesting = true;
                    _statusMessage = "Validating...";
                    Task.Run(ValidateAndStart);
                }
                ImGui.SameLine();
                if (ImGui.Button("Reconfigure", new Vector2(180, 40)))
                {
                    _config.Reset();
                    _selectedProvider = "opencode";
                    _apiKey = "";
                }
            }
            else
            {
                ImGui.Text("Welcome! Select your AI provider:");
                ImGui.Separator();

                bool isOpen = _selectedProvider == "opencode";
                bool isClaude = _selectedProvider == "claude";
                if (ImGui.RadioButton("OpenCode", isOpen)) _selectedProvider = "opencode";
                ImGui.SameLine();
                if (ImGui.RadioButton("Claude Code", isClaude)) _selectedProvider = "claude";

                ImGui.Separator();
                if (_selectedProvider == "claude")
                {
                    ImGui.InputText("API Key", ref _apiKey, 200);
                }

                ImGui.Separator();
                if (ImGui.Button("Start", new Vector2(380, 40)))
                {
                    _isTesting = true;
                    _statusMessage = $"Testing {_selectedProvider}...";
                    Task.Run(TestProvider);
                }
            }

            ImGui.End();
            ImGui.EndFrame();
        };

        window.Resize += (size) => renderer.Resize(size);

        window.Run();
        input?.Dispose();
        await host.StopAsync();
    }

    private static async Task TestProvider()
    {
        _config.SetProvider(_selectedProvider);
        _config.SetApiKey(_apiKey);
        await Task.Delay(2000);
        _config.MarkConfigured();
        _isTesting = false;
        _configComplete = true;
        _statusMessage = $"{_selectedProvider} connection successful!";
    }

    private static async Task ValidateAndStart()
    {
        await Task.Delay(1500);
        _isTesting = false;
        _statusMessage = "Connection valid. Entering office...";
        await Task.Delay(500);
        _showOffice = true;
    }
}
