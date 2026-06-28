using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Domain.Agents;

public sealed class Meeting : AggregateRoot<Guid>
{
    public string Topic { get; private set; }
    public List<AgentId> Participants { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public MeetingStatus Status { get; private set; }
    public List<string> Decisions { get; private set; }

    private Meeting() : base(Guid.NewGuid()) { }

    public static Meeting Schedule(string topic, List<AgentId> participants, DateTime? scheduledAt = null)
    {
        return new Meeting
        {
            Topic = topic,
            Participants = participants,
            ScheduledAt = scheduledAt ?? DateTime.UtcNow.AddMinutes(5),
            Status = MeetingStatus.Scheduled,
            Decisions = []
        };
    }

    public void Start() => Status = MeetingStatus.InProgress;
    public void End() => Status = MeetingStatus.Completed;
    public void AddDecision(string decision) => Decisions.Add(decision);
}

public enum MeetingStatus { Scheduled, InProgress, Completed }
