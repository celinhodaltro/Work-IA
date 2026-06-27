namespace Work_IA.Application.Common.Interfaces;

public interface IEventTypeResolver
{
    Type? ResolveType(string eventTypeName);
}
