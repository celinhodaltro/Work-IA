using Work_IA.Domain.Abstractions;

namespace Work_IA.Domain.Agents;

public sealed class AgentPermissions : ValueObject
{
    public bool CanRead { get; }
    public bool CanWrite { get; }
    public bool CanDelegate { get; }
    public AgentId? ReportsTo { get; }

    public AgentPermissions(bool canRead = true, bool canWrite = true, bool canDelegate = false, AgentId? reportsTo = null)
    {
        CanRead = canRead;
        CanWrite = canWrite;
        CanDelegate = canDelegate;
        ReportsTo = reportsTo;
    }

    public bool CanAssignTaskTo(Agent target)
    {
        if (!CanDelegate) return false;
        if (target.Permissions.ReportsTo is null) return false;
        return target.Permissions.ReportsTo == ReportsTo;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return CanRead;
        yield return CanWrite;
        yield return CanDelegate;
        yield return ReportsTo ?? AgentId.From(Guid.Empty);
    }
}
