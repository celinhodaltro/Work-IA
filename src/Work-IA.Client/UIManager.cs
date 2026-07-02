using ImGuiNET;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using Work_IA.Application.Agents;
using Work_IA.Domain.Agents;

namespace Work_IA.Client;

public sealed class UIManager
{
    private readonly AgentRegistry _registry;
    private readonly AgentStateEventHandler _eventHandler;
    private readonly ConfigManager _config;
    private ImGuiController? _controller;
    private bool _showAgents = true;
    private bool _showHire = false;
    private string _newName = "";
    private string _newTitle = "";
    private string _log = "";

    private bool _launcherMode = true;
    private string _selectedProvider = "opencode";
    private string _apiKey = "";
    private bool _isTesting;
    private string _statusMessage = "";

    public bool IsLauncherMode => _launcherMode;

    public UIManager(AgentRegistry registry, AgentStateEventHandler eventHandler, ConfigManager config)
    {
        _registry = registry;
        _eventHandler = eventHandler;
        _config = config;
        _selectedProvider = _config.Config.AiProvider;
        _apiKey = _config.Config.ApiKey;
        if (_config.IsConfigured) _launcherMode = false;
    }

    public void Initialize(GL gl, IView view)
    {
        var input = view.CreateInput();
        _controller = new ImGuiController(gl, view, input);
    }

    public void Update(float delta)
    {
        _controller?.Update(delta);
    }

    public void Render()
    {
        ImGuiNET.ImGui.NewFrame();

        if (_launcherMode)
            RenderLauncher();
        else
            RenderOfficeUI();

        ImGuiNET.ImGui.EndFrame();
        _controller?.Render();
    }

    private void RenderLauncher()
    {
        var center = new Vector2(400, 300);
        ImGui.SetNextWindowPos(new Vector2(center.X - 200, center.Y - 200), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(400, 350));
        ImGui.Begin("AI Office OS", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove);

        if (_isTesting)
        {
            ImGui.Text("Testing connection...");
            ImGui.Text(_statusMessage);
        }
        else if (_config.IsConfigured)
        {
            ImGui.Text("AI Office OS");
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
                _config.SetProvider(_selectedProvider);
                _config.SetApiKey(_apiKey);
                _isTesting = true;
                _statusMessage = $"Testing {_selectedProvider}...";
                Task.Run(TestProvider);
            }
        }

        ImGui.End();
    }

    private void RenderOfficeUI()
    {
        ImGui.Begin("AI Office OS", ImGuiWindowFlags.AlwaysAutoResize);
        ImGui.Text($"Agents: {_registry.GetAll().Count(a => a.Status == AgentStatus.Running)} online");
        ImGui.Separator();

        if (ImGui.Button("Hire Agent")) _showHire = !_showHire;
        ImGui.SameLine();
        if (ImGui.Button("Agents")) _showAgents = !_showAgents;

        if (_showHire)
        {
            ImGui.Begin("Hire Agent", ImGuiWindowFlags.AlwaysAutoResize);
            ImGui.InputText("Name", ref _newName, 100);
            ImGui.InputText("Title", ref _newTitle, 100);
            if (ImGui.Button("Hire") && !string.IsNullOrEmpty(_newName))
            {
                _log = $"Hired {_newName} as {_newTitle}";
                _newName = ""; _newTitle = "";
            }
            ImGui.End();
        }

        if (_showAgents)
        {
            ImGui.Begin("Agents", ImGuiWindowFlags.AlwaysAutoResize);
            foreach (var a in _eventHandler.Agents)
            {
                ImGui.PushID(a.Id.ToString());
                ImGui.Text($"{a.GetEmoji()} {a.Name} - {a.Title}");
                ImGui.Text($"  {a.Emotion} | {a.CurrentAction}");
                if (a.ConversationTopic is not null)
                    ImGui.Text($"  💬 {a.ConversationTopic}");
                ImGui.Separator();
                ImGui.PopID();
            }
            ImGui.End();
        }

        if (!string.IsNullOrEmpty(_log))
        {
            ImGui.Begin("Log"); ImGui.Text(_log); ImGui.End();
        }
        ImGui.End();
    }

    private async Task TestProvider()
    {
        await Task.Delay(2000);
        _config.MarkConfigured();
        _isTesting = false;
        _launcherMode = false;
    }

    private async Task ValidateAndStart()
    {
        await Task.Delay(1500);
        _isTesting = false;
        _launcherMode = false;
    }

    public void Dispose() => _controller?.Dispose();
}
