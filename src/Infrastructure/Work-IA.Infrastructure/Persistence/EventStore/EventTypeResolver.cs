using Work_IA.Application.Common.Interfaces;

namespace Work_IA.Infrastructure.Persistence.EventStore;

public sealed class EventTypeResolver : IEventTypeResolver
{
    private readonly Dictionary<string, Type> _types;

    public EventTypeResolver(IEnumerable<Type> knownEventTypes)
    {
        _types = knownEventTypes.ToDictionary(t => t.FullName!, t => t);
    }

    public Type? ResolveType(string eventTypeName)
    {
        return _types.TryGetValue(eventTypeName, out var type) ? type : null;
    }
}
