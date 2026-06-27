using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Events;

namespace Work_IA.Domain.Agents;

public sealed class Agent : AggregateRoot<AgentId>
{
    private readonly List<ObservationRule> _observationRules = [];
    private readonly List<AgentMessage> _inbox = [];

    public AgentName Name { get; private set; }
    public AgentRole Role { get; private set; }
    public AgentStatus Status { get; private set; }
    public IReadOnlyList<ObservationRule> ObservationRules => _observationRules.AsReadOnly();
    public IReadOnlyList<AgentMessage> Inbox => _inbox.AsReadOnly();
    public DateTime? LastHeartbeat { get; private set; }

    private Agent(AgentId id, AgentName name, AgentRole role) : base(id)
    {
        Name = name;
        Role = role;
        Status = AgentStatus.Created;
        LastHeartbeat = DateTime.UtcNow;
    }

    public static Agent Create(AgentName name, AgentRole role)
    {
        var agent = new Agent(AgentId.New(), name, role);
        agent.RaiseDomainEvent(new AgentCreatedDomainEvent(agent.Id, name.Value, role));
        return agent;
    }

    public Task StartAsync()
    {
        if (Status != AgentStatus.Created && Status != AgentStatus.Stopped)
            throw new InvalidOperationException($"Cannot start agent in {Status} status");

        Status = AgentStatus.Running;
        LastHeartbeat = DateTime.UtcNow;
        RaiseDomainEvent(new AgentStartedDomainEvent(Id));
        return Task.CompletedTask;
    }

    public Task PauseAsync()
    {
        if (Status != AgentStatus.Running)
            throw new InvalidOperationException($"Cannot pause agent in {Status} status");

        Status = AgentStatus.Paused;
        RaiseDomainEvent(new AgentPausedDomainEvent(Id));
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        Status = AgentStatus.Stopped;
        RaiseDomainEvent(new AgentStoppedDomainEvent(Id));
        return Task.CompletedTask;
    }

    public void AddObservationRule(ObservationRule rule)
    {
        _observationRules.Add(rule);
        RaiseDomainEvent(new ObservationRuleAddedDomainEvent(Id, rule.EventType));
    }

    public bool ShouldObserve(string eventType, object? eventData = null)
    {
        return _observationRules.Any(r =>
            r.EventType == eventType &&
            (r.Condition is null || r.Condition(eventData!)));
    }

    public void ReceiveMessage(AgentMessage message)
    {
        _inbox.Add(message);
        RaiseDomainEvent(new MessageReceivedDomainEvent(Id, message.From));
    }

    public void UpdateHeartbeat()
    {
        LastHeartbeat = DateTime.UtcNow;
    }

    public bool IsOnline => Status == AgentStatus.Running &&
                            LastHeartbeat.HasValue &&
                            (DateTime.UtcNow - LastHeartbeat.Value).TotalMinutes < 5;
}
