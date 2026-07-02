using MediatR;
using Work_IA.Domain.Events;
using Work_IA.Client.Models;

namespace Work_IA.Client;

public sealed class AgentStateEventHandler : INotificationHandler<AgentStateChangedEvent>
{
    public List<VisualAgent> Agents { get; } = [];
    public event Action? OnAgentsUpdated;

    public Task Handle(AgentStateChangedEvent notification, CancellationToken cancellationToken)
    {
        var agent = Agents.FirstOrDefault(a => a.Id == notification.AgentId.Value);
        if (agent is null)
        {
            agent = new VisualAgent { Id = notification.AgentId.Value };
            Agents.Add(agent);
        }
        agent.Name = notification.Name;
        agent.Title = notification.Title;
        agent.Status = notification.Status;
        agent.Emotion = notification.Emotion;
        agent.CurrentAction = notification.Action;
        agent.ConversationTopic = notification.ConversationTopic;
        agent.TargetX = notification.PositionX;
        agent.TargetY = notification.PositionY;
        agent.CurrentX = notification.PositionX;
        agent.CurrentY = notification.PositionY;

        OnAgentsUpdated?.Invoke();
        return Task.CompletedTask;
    }
}
