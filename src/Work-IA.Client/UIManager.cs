using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using Work_IA.Application.Agents;
using Work_IA.Domain.Agents;

namespace Work_IA.Client;

public sealed class UIManager : IDisposable
{
    private readonly AgentRegistry _registry;
    private readonly AgentStateEventHandler _eventHandler;
    private ImGuiController? _controller;
    private bool _showHire = false;
    private string _newName = "";
    private string _newTitle = "";
    private bool _newCanRead = true;
    private bool _newCanWrite = true;
    private bool _newCanDelegate = false;
    private string _newReportsTo = "";
    private string _log = "";

    public UIManager(AgentRegistry registry, AgentStateEventHandler eventHandler)
    {
        _registry = registry;
        _eventHandler = eventHandler;
    }

    public void Initialize(GL gl, IView view, IInputContext input)
    {
        _controller = new ImGuiController(gl, view, input);
    }

    public void Update(float delta) => _controller?.Update(delta);

    public void Render()
    {
        RenderUI();
        _controller?.Render();
    }

    private void RenderUI()
    {
        var agents = _registry.GetAll().ToList();
        var io = ImGui.GetIO();
        var panelW = 340f;
        var panelX = io.DisplaySize.X - panelW - 10;
        var panelY = io.DisplaySize.Y - 350;

        ImGui.SetNextWindowPos(new System.Numerics.Vector2(panelX, panelY), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(panelW, 340));
        ImGui.Begin("AI Office OS", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
        ImGui.Text($"Agents: {agents.Count(a => a.Status == AgentStatus.Running)} online");
        ImGui.Separator();

        if (ImGui.Button("Hire Agent", new System.Numerics.Vector2(150, 28))) _showHire = !_showHire;
        ImGui.SameLine();
        if (ImGui.Button("Refresh", new System.Numerics.Vector2(150, 28))) { }

        if (_showHire)
        {
            ImGui.Separator();
            ImGui.Text("Hire New Agent");
            ImGui.InputText("Name", ref _newName, 100);
            ImGui.InputText("Title", ref _newTitle, 100);

            ImGui.Text("Reports To:");
            var head = agents.Select(a => a is AgentBase ab ? ab.Agent : null).FirstOrDefault(a => a?.IsHead == true);
            var headName = head?.Name.Value ?? "None";
            if (ImGui.BeginCombo("##reports", _newReportsTo == "" ? "None" : _newReportsTo))
            {
                if (ImGui.Selectable("None", _newReportsTo == "")) _newReportsTo = "";
                foreach (var a in agents)
                {
                    if (ImGui.Selectable(a.Name, _newReportsTo == a.Name))
                        _newReportsTo = a.Name;
                }
                ImGui.EndCombo();
            }

            ImGui.Checkbox("Can Read", ref _newCanRead);
            ImGui.Checkbox("Can Write", ref _newCanWrite);
            ImGui.Checkbox("Can Delegate", ref _newCanDelegate);

            if (ImGui.Button("Hire", new System.Numerics.Vector2(300, 32)) && !string.IsNullOrEmpty(_newName))
            {
                _log = $"Hired {_newName} as {_newTitle}";
                _newName = ""; _newTitle = "";
            }
        }

        ImGui.Separator();
        ImGui.Text("Team:");
        ImGui.BeginChild("AgentList", new System.Numerics.Vector2(320, 150));
        foreach (var ia in agents)
        {
            ImGui.PushID(ia.AgentId.Value.ToString());
            var agent = ia is AgentBase ab ? ab.Agent : null;
            if (agent is null) { ImGui.PopID(); continue; }
            var icon = agent.IsHead ? "👑" : agent.Status == AgentStatus.Running ? "🟢" : "⚪";
            ImGui.Text($"{icon} {agent.Name.Value}");
            ImGui.SameLine(200);
            ImGui.TextColored(new System.Numerics.Vector4(0.6f, 0.6f, 0.7f, 1), agent.Title.Value);
            if (agent.IsHead) { ImGui.SameLine(); ImGui.TextColored(new System.Numerics.Vector4(1, 0.8f, 0, 1), " HEAD"); }
            if (ImGui.IsItemHovered())
            {
                var perms = agent.Permissions;
                ImGui.BeginTooltip();
                ImGui.Text($"{agent.Emotion} | {agent.CurrentAction}");
                ImGui.Text($"Read: {(perms.CanRead ? "✅" : "❌")} | Write: {(perms.CanWrite ? "✅" : "❌")} | Delegate: {(perms.CanDelegate ? "✅" : "❌")}");
                if (perms.ReportsTo is not null)
                {
                    var boss = agents.FirstOrDefault(b => b.AgentId == perms.ReportsTo);
                    ImGui.Text($"Reports to: {boss?.Name ?? "Unknown"}");
                }
                ImGui.EndTooltip();
            }
            ImGui.Separator();
            ImGui.PopID();
        }
        ImGui.EndChild();

        if (!string.IsNullOrEmpty(_log)) { ImGui.TextColored(new System.Numerics.Vector4(0.5f, 0.8f, 0.5f, 1), _log); }
        ImGui.End();
    }

    public void Dispose() => _controller?.Dispose();
}
