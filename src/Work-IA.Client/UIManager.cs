using ImGuiNET;
using System.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using Silk.NET.Input;
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

    public void Initialize(GL gl, IView view, IInputContext input)
    {
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
        var io = ImGui.GetIO();
        var w = io.DisplaySize.X;
        var h = io.DisplaySize.Y;

        var bgColor = ImGui.GetColorU32(new Vector4(0.06f, 0.06f, 0.10f, 1f));
        var topBarColor = ImGui.GetColorU32(new Vector4(0.10f, 0.10f, 0.14f, 1f));
        var accentColor = ImGui.GetColorU32(new Vector4(0.25f, 0.45f, 0.85f, 1f));
        var textColor = ImGui.GetColorU32(new Vector4(0.95f, 0.95f, 1f, 1f));

        var draw = ImGui.GetForegroundDrawList();
        draw.AddRectFilled(Vector2.Zero, new Vector2(w, h), bgColor);
        draw.AddRectFilled(new Vector2(0, 0), new Vector2(w, 56), topBarColor);
        draw.AddRectFilled(new Vector2(0, 56), new Vector2(w, 58), accentColor);

        draw.AddText(new Vector2(24, 16), ImGui.GetColorU32(new Vector4(0.3f, 0.6f, 1f, 1)), "AI Office OS");

        var panelW = 380f;
        var panelX = (w - panelW) / 2;
        var panelY = 120f;

        if (_isTesting)
        {
            draw.AddText(new Vector2(panelX, panelY), textColor, "Testing connection...");
            draw.AddText(new Vector2(panelX, panelY + 30), textColor, _statusMessage);
        }
        else if (_config.IsConfigured)
        {
            draw.AddText(new Vector2(panelX, panelY), ImGui.GetColorU32(new Vector4(0.3f, 0.8f, 0.3f, 1)), "✓ Configured");
            draw.AddText(new Vector2(panelX, panelY + 30), textColor, $"Provider: {_config.Config.AiProvider}");

            ImGui.SetCursorPos(new Vector2(panelX, h - 100));
            if (ImGui.Button("Continue", new Vector2(170, 44)))
            {
                _isTesting = true;
                _statusMessage = "Validating...";
                Task.Run(ValidateAndStart);
            }
            ImGui.SameLine();
            if (ImGui.Button("Reconfigure", new Vector2(170, 44)))
            {
                _config.Reset();
                _selectedProvider = "opencode";
                _apiKey = "";
            }
        }
        else
        {
            draw.AddText(new Vector2(panelX, panelY), ImGui.GetColorU32(new Vector4(0.7f, 0.7f, 0.8f, 1)), "Select AI Provider");

            ImGui.SetCursorPos(new Vector2(panelX, panelY + 50));
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(12, 8));
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 12));

            ImGui.SetCursorPos(new Vector2(panelX, panelY + 80));
            ImGui.Text(_selectedProvider == "opencode" ? "● OpenCode" : "○ OpenCode");
            if (ImGui.IsItemClicked()) _selectedProvider = "opencode";

            ImGui.SetCursorPos(new Vector2(panelX, panelY + 110));
            ImGui.Text(_selectedProvider == "claude" ? "● Claude Code" : "○ Claude Code");
            if (ImGui.IsItemClicked()) _selectedProvider = "claude";

            ImGui.PopStyleVar(2);

            if (_selectedProvider == "claude")
            {
                ImGui.SetCursorPos(new Vector2(panelX, panelY + 150));
                ImGui.InputText("API Key", ref _apiKey, 200);
            }

            ImGui.SetCursorPos(new Vector2(panelX, h - 100));
            if (ImGui.Button("Start", new Vector2(380, 48)))
            {
                _config.SetProvider(_selectedProvider);
                _config.SetApiKey(_apiKey);
                _isTesting = true;
                _statusMessage = $"Testing {_selectedProvider}...";
                Task.Run(TestProvider);
            }
        }
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
