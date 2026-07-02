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
    private string _chatMessage = "";
    private string _renameText = "";
    private bool _showRename = false;

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
        var io = ImGui.GetIO();
        var agents = _registry.GetAll().Select(a => a is AgentBase ab ? ab.Agent : null).Where(a => a is not null).Cast<Agent>().ToList();
        var panelW = 340f;
        var panelX = io.DisplaySize.X - panelW - 10;
        var panelY = io.DisplaySize.Y - 310;

        ImGui.SetNextWindowPos(new Vector2(panelX, panelY), ImGuiCond.Always);
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
            var icon = agent.IsHead ? "👑" : agent.Status == AgentStatus.Running ? "🟢" : "⚪";
            ImGui.Selectable($"{icon} {agent.Name.Value}  ({agent.Title.Value})", _selectedAgent?.AgentId == agent.AgentId);
            if (ImGui.IsItemClicked()) _selectedAgent = _selectedAgent?.AgentId == agent.AgentId ? null : agent;
            if (ImGui.IsItemHovered())
            {
                var p = agent.Permissions;
                ImGui.BeginTooltip();
                ImGui.Text($"Emotion: {agent.Emotion}  |  Action: {agent.CurrentAction}");
                ImGui.Text($"Read: {(p.CanRead ? "✅" : "❌")}  |  Write: {(p.CanWrite ? "✅" : "❌")}  |  Delegate: {(p.CanDelegate ? "✅" : "❌")}");
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

        if (_showHire) RenderHireModal(agents);
        if (_selectedAgent is not null) RenderChatModal(_selectedAgent, agents);
    }

    private void RenderHireModal(List<Agent> agents)
    {
        var io = ImGui.GetIO();
        ImGui.SetNextWindowPos(new Vector2(io.DisplaySize.X / 2 - 175, io.DisplaySize.Y / 2 - 200), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(350, 400));
        ImGui.Begin("Hire New Agent", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);

        ImGui.InputText("Name", ref _newName, 100);
        ImGui.InputText("Title", ref _newTitle, 100);

        ImGui.Text("Reports To:");
        var preview = _newReportsToIdx >= 0 && _newReportsToIdx < agents.Count ? agents[_newReportsToIdx].Name.Value : "None";
        if (ImGui.BeginCombo("##reports", preview))
        {
            if (ImGui.Selectable("None", _newReportsToIdx == -1)) _newReportsToIdx = -1;
            for (int i = 0; i < agents.Count; i++)
            {
                if (ImGui.Selectable(agents[i].Name.Value, _newReportsToIdx == i)) _newReportsToIdx = i;
            }
            ImGui.EndCombo();
        }

        ImGui.Checkbox("Can Read", ref _newCanRead);
        ImGui.Checkbox("Can Write", ref _newCanWrite);
        ImGui.Checkbox("Can Delegate", ref _newCanDelegate);

        ImGui.Dummy(new Vector2(0, 10));
        if (ImGui.Button("Hire", new Vector2(310, 36)) && !string.IsNullOrEmpty(_newName))
        {
            _log = $"Hired {_newName}";
            _newName = ""; _newTitle = "";
            _newReportsToIdx = -1;
            _showHire = false;
        }
        ImGui.SameLine();
        if (ImGui.Button("Cancel", new Vector2(310, 36))) _showHire = false;

        ImGui.End();
    }

    private void RenderChatModal(Agent agent, List<Agent> allAgents)
    {
        var io = ImGui.GetIO();
        var chatW = 400; var chatH = 300;
        ImGui.SetNextWindowPos(new Vector2(io.DisplaySize.X / 2 - chatW / 2, io.DisplaySize.Y / 2 - chatH / 2), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(chatW, chatH));

        var name = agent.Name.Value;
        var title = agent.Title.Value;
        var perms = agent.Permissions;

        ImGui.Begin($"{name} - {title}", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
        ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.8f, 1), $"Level: {agent.CareerLevel}  |  XP: {agent.ExperiencePoints}  |  Status: {agent.Status}");
        ImGui.Separator();

        if (ImGui.Button($"✏️ Rename")) { _renameText = agent.Name.Value; _showRename = true; }
        ImGui.SameLine();
        ImGui.Text($"Read: {(perms.CanRead ? "✅" : "❌")}  Write: {(perms.CanWrite ? "✅" : "❌")}  Delegate: {(perms.CanDelegate ? "✅" : "❌")}");
        if (perms.ReportsTo is not null)
        {
            var boss = allAgents.FirstOrDefault(b => b.AgentId == perms.ReportsTo);
            ImGui.SameLine();
            ImGui.Text($"  Reports to: {boss?.Name.Value ?? "Unknown"}");
        }

        ImGui.Separator();
        ImGui.Text($"Emotion: {agent.Emotion}  |  Action: {agent.CurrentAction}");
        if (agent.ConversationTopic is not null) ImGui.Text($"💬 Discussing: {agent.ConversationTopic}");

        ImGui.Separator();
        ImGui.Text("Chat:");
        ImGui.InputTextMultiline("##chatlog", ref _chatMessage, 500, new Vector2(370, 80), ImGuiInputTextFlags.ReadOnly);
        ImGui.End();

        if (_showRename)
        {
            ImGui.OpenPopup("RenameAgent");
            ImGui.BeginPopupModal("RenameAgent", ref _showRename, ImGuiWindowFlags.AlwaysAutoResize);
            ImGui.Text($"Rename {name} to:");
            ImGui.InputText("##newname", ref _renameText, 100);
            if (ImGui.Button("Save"))
            {
                _log = $"Renamed {_renameText}";
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel")) ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
        }
    }

    public void Dispose() => _controller?.Dispose();
}
