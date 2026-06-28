using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Events;
using Work_IA.Domain.Agents;

namespace Work_IA.Domain.Agents;

public sealed class Agent : AggregateRoot<AgentId>
{
    private readonly List<ObservationRule> _observationRules = [];
    private readonly List<AgentMessage> _inbox = [];

    public AgentId AgentId { get; private set; }
    public AgentName Name { get; private set; }
    public AgentTitle Title { get; private set; }
    public AgentCareerLevel CareerLevel { get; private set; }
    public RoleId? RoleId { get; private set; }
    public AgentStatus Status { get; private set; }
    public int ExperiencePoints { get; private set; }
    public List<AgentSkill> Skills { get; private set; }
    public AgentId? MentorId { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LastPromotionAt { get; private set; }
    public DateTime? LastHeartbeat { get; private set; }

    public IReadOnlyList<ObservationRule> ObservationRules => _observationRules.AsReadOnly();
    public IReadOnlyList<AgentMessage> Inbox => _inbox.AsReadOnly();

    public AgentAction CurrentAction { get; set; } = AgentAction.Idle;
    public float ActionTimer { get; set; }
    public AgentEmotion Emotion { get; set; } = AgentEmotion.Neutral;
    public string? ConversationTopic { get; set; }
    public AgentId? ConversationPartner { get; set; }
    public AgentPosition Position { get; set; } = new(0, 0, 0);

    private Agent() : base(AgentId.New()) { }

    public Agent(AgentId id, AgentName name, AgentTitle title, AgentCareerLevel careerLevel) : base(id)
    {
        AgentId = id;
        Name = name;
        Title = title;
        CareerLevel = careerLevel;
        Status = AgentStatus.Created;
        ExperiencePoints = 0;
        Skills = [];
        JoinedAt = DateTime.UtcNow;
    }

    public static Agent Create(AgentName name, AgentTitle title, RoleId? roleId = null)
    {
        var agent = new Agent(AgentId.New(), name, title, AgentCareerLevel.Intern)
        {
            Status = AgentStatus.Created,
            ExperiencePoints = 0,
            Skills = [],
            RoleId = roleId,
            JoinedAt = DateTime.UtcNow
        };
        agent.RaiseDomainEvent(new AgentCreatedDomainEvent(agent.AgentId, name.Value, title.Value));
        return agent;
    }

    public void Start()
    {
        if (Status != AgentStatus.Created && Status != AgentStatus.Stopped)
            throw new InvalidOperationException($"Cannot start agent in {Status} status");
        Status = AgentStatus.Running;
        UpdateHeartbeat();
        RaiseDomainEvent(new AgentStartedDomainEvent(AgentId));
    }

    public void Pause()
    {
        if (Status != AgentStatus.Running)
            throw new InvalidOperationException($"Cannot pause agent in {Status} status");
        Status = AgentStatus.Paused;
        RaiseDomainEvent(new AgentPausedDomainEvent(AgentId));
    }

    public void Stop()
    {
        Status = AgentStatus.Stopped;
        RaiseDomainEvent(new AgentStoppedDomainEvent(AgentId));
    }

    public void AssignSkill(AgentSkill skill)
    {
        if (!Skills.Any(s => s.Name.Equals(skill.Name, StringComparison.OrdinalIgnoreCase)))
        {
            Skills.Add(skill);
        }
    }

    public void AddExperience(int points)
    {
        if (points <= 0)
            throw new ArgumentException("Experience points must be positive", nameof(points));
        ExperiencePoints += points;
        RaiseDomainEvent(new ExperienceGainedDomainEvent(AgentId, points, ExperiencePoints));
    }

    public bool CanPromoteTo(AgentCareerLevel targetLevel, int xpRequired)
    {
        return targetLevel > CareerLevel && ExperiencePoints >= xpRequired;
    }

    public void Promote(AgentTitle newTitle, AgentCareerLevel newLevel)
    {
        if (newLevel <= CareerLevel)
            throw new InvalidOperationException($"Cannot promote to same or lower level. Current: {CareerLevel}, Target: {newLevel}");
        Title = newTitle;
        CareerLevel = newLevel;
        LastPromotionAt = DateTime.UtcNow;
        RaiseDomainEvent(new AgentPromotedDomainEvent(AgentId, newTitle.Value, newLevel));
    }

    public void AssignMentor(AgentId mentorId)
    {
        MentorId = mentorId;
    }

    public void AddObservationRule(ObservationRule rule)
    {
        _observationRules.Add(rule);
        RaiseDomainEvent(new ObservationRuleAddedDomainEvent(AgentId, rule.EventType));
    }

    public bool ShouldObserve(string eventType)
    {
        return _observationRules.Any(r => r.EventType == eventType);
    }

    public void ReceiveMessage(AgentMessage message)
    {
        _inbox.Add(message);
        RaiseDomainEvent(new MessageReceivedDomainEvent(AgentId, message.From));
    }

    public void UpdateHeartbeat()
    {
        LastHeartbeat = DateTime.UtcNow;
    }

    public bool IsOnline => Status == AgentStatus.Running &&
                            LastHeartbeat.HasValue &&
                            (DateTime.UtcNow - LastHeartbeat.Value).TotalMinutes < 5;
}

