namespace Work_IA.Domain.Abstractions;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : struct, IEquatable<TId>
{
    private int _version;

    public int Version => _version;

    protected AggregateRoot(TId id) : base(id) { }
    protected AggregateRoot() { }

    protected void IncrementVersion()
    {
        _version++;
    }
}
