using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
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
    private ImGuiController? _controller;
    private bool _showAgents = true;
    private bool _showHire = false;
    private string _newName = "";
    private string _newTitle = "";
    private string _log = "";

    public UIManager(AgentRegistry registry, AgentStateEventHandler eventHandler)
    {
        _registry = registry;
        _eventHandler = eventHandler;
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
        ImGuiNET.ImGui.EndFrame();
        _controller?.Render();
    }

    public void Dispose() => _controller?.Dispose();
}
