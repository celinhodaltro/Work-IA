using ImGuiNET;
using System.Numerics;
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
    private int _newReportsToIdx = -1;
    private string _log = "";
    private Agent? _selectedAgent;
    private bool _showRename = false;
    private string _renameText = "";

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
        var agents = _registry.GetAll().Select(a => a is AgentBase ab ? ab.Agent : null).Where(a => a is not null).Cast<Agent>().ToList();
        RenderMainPanel(agents);
        if (_showHire) RenderHireModal(agents);
        if (_selectedAgent is not null) RenderAgentDetail(_selectedAgent, agents);
        if (_showRename) RenderRenameModal();
        _controller?.Render();
    }

    private void RenderMainPanel(List<Agent> agents)
    {
        var io = ImGui.GetIO();
        var panelW = 340f;
        ImGui.SetNextWindowPos(new Vector2(io.DisplaySize.X - panelW - 10, io.DisplaySize.Y - 310), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(panelW, 300));
        ImGui.Begin("AI Office OS", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
        ImGui.Text($"Agents: {agents.Count(a => a.Status == AgentStatus.Running)} online");
        ImGui.Separator();
        if (ImGui.Button("Hire Agent", new Vector2(310, 28))) _showHire = true;
        ImGui.Separator();
        ImGui.Text("Team:");
        ImGui.BeginChild("AgentList", new Vector2(320, 190));
        foreach (var agent in agents)
        {
            ImGui.PushID(agent.AgentId.Value.ToString());
            var icon = agent.IsHead ? "\U0001F451" : agent.Status == AgentStatus.Running ? "\U0001F7E2" : "\u26AA";
            var isSelected = _selectedAgent?.AgentId == agent.AgentId;
            if (ImGui.Selectable($"{icon} {agent.Name.Value}  ({agent.Title.Value})", isSelected))
                _selectedAgent = isSelected ? null : agent;
            if (ImGui.IsItemHovered())
            {
                var p = agent.Permissions;
                ImGui.BeginTooltip();
                ImGui.Text($"Emotion: {agent.Emotion}  |  Action: {agent.CurrentAction}");
                ImGui.Text($"Read: {(p.CanRead ? "OK" : "NO")}  |  Write: {(p.CanWrite ? "OK" : "NO")}  |  Delegate: {(p.CanDelegate ? "OK" : "NO")}");
                if (p.ReportsTo is not null)
                {
                    var boss = agents.FirstOrDefault(b => b.AgentId == p.ReportsTo);
                    ImGui.Text($"Reports to: {boss?.Name.Value ?? "Unknown"}");
                }
                ImGui.EndTooltip();
            }
            ImGui.PopID();
        }
        ImGui.EndChild();
        if (!string.IsNullOrEmpty(_log)) ImGui.TextColored(new Vector4(0.5f, 0.8f, 0.5f, 1), _log);
        ImGui.End();
    }

    private void RenderHireModal(List<Agent> agents)
    {
        var io = ImGui.GetIO();
        ImGui.SetNextWindowPos(new Vector2(io.DisplaySize.X / 2 - 175, io.DisplaySize.Y / 2 - 200), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(350, 380));
        ImGui.Begin("Hire New Agent", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
        ImGui.InputText("Name", ref _newName, 100);
        ImGui.InputText("Title", ref _newTitle, 100);
        ImGui.Text("Reports To:");
        var preview = _newReportsToIdx >= 0 && _newReportsToIdx < agents.Count ? agents[_newReportsToIdx].Name.Value : "None";
        if (ImGui.BeginCombo("##reports", preview))
        {
            if (ImGui.Selectable("None", _newReportsToIdx == -1)) _newReportsToIdx = -1;
            for (int i = 0; i < agents.Count; i++)
                if (ImGui.Selectable(agents[i].Name.Value, _newReportsToIdx == i)) _newReportsToIdx = i;
            ImGui.EndCombo();
        }
        ImGui.Checkbox("Can Read", ref _newCanRead);
        ImGui.Checkbox("Can Write", ref _newCanWrite);
        ImGui.Checkbox("Can Delegate", ref _newCanDelegate);
        ImGui.Dummy(new Vector2(0, 10));
        if (ImGui.Button("Hire", new Vector2(310, 36)) && !string.IsNullOrEmpty(_newName))
        {
            _log = $"Hired {_newName}";
            _newName = ""; _newTitle = ""; _newReportsToIdx = -1;
            _showHire = false;
        }
        if (ImGui.Button("Cancel", new Vector2(310, 36))) _showHire = false;
        ImGui.End();
    }

    private void RenderAgentDetail(Agent agent, List<Agent> allAgents)
    {
        var io = ImGui.GetIO();
        ImGui.SetNextWindowPos(new Vector2(io.DisplaySize.X / 2 - 200, io.DisplaySize.Y / 2 - 150), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(400, 280));
        ImGui.Begin($"{agent.Name.Value} - {agent.Title.Value}", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
        ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.8f, 1), $"Status: {agent.Status}  |  Emotion: {agent.Emotion}  |  Action: {agent.CurrentAction}");
        ImGui.Separator();
        if (ImGui.Button("\u270F\uFE0F Rename")) { _renameText = agent.Name.Value; _showRename = true; }
        ImGui.SameLine();
        var p = agent.Permissions;
        ImGui.Text($"Read: {(p.CanRead ? "OK" : "NO")}  Write: {(p.CanWrite ? "OK" : "NO")}  Delegate: {(p.CanDelegate ? "OK" : "NO")}");
        if (p.ReportsTo is not null)
        {
            var boss = allAgents.FirstOrDefault(b => b.AgentId == p.ReportsTo);
            ImGui.SameLine(); ImGui.Text($"  Reports to: {boss?.Name.Value ?? "Unknown"}");
        }
        if (agent.ConversationTopic is not null) ImGui.Text($"Discussing: {agent.ConversationTopic}");
        ImGui.Separator();
        ImGui.Text("Chat:");
        ImGui.InputTextMultiline("##chatlog", ref _log, 1000, new Vector2(370, 80), ImGuiInputTextFlags.ReadOnly);
        ImGui.Separator();
        if (ImGui.Button("Close", new Vector2(370, 30))) _selectedAgent = null;
        ImGui.End();
    }

    private void RenderRenameModal()
    {
        ImGui.OpenPopup("Rename##popup");
        if (ImGui.BeginPopupModal("Rename##popup", ref _showRename, ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text("New name:");
            ImGui.InputText("##name", ref _renameText, 100);
            if (ImGui.Button("Save")) { _log = $"Renamed to {_renameText}"; _showRename = false; ImGui.CloseCurrentPopup(); }
            ImGui.SameLine();
            if (ImGui.Button("Cancel")) { _showRename = false; ImGui.CloseCurrentPopup(); }
            if (!_showRename) ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
        }
        else _showRename = false;
    }

    public void Dispose() => _controller?.Dispose();
}
