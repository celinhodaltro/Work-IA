using Work_IA.Domain.Workspace;
using Work_IA.Application.Common.Interfaces;

namespace Work_IA.Application.Common.Events;

public sealed record WorkspaceFileChange : IIntegrationEvent
{
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public string EventType => GetType().Name;
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public string Path { get; init; } = string.Empty;
    public FileChangeType ChangeType { get; init; }
    public string? OldContent { get; init; }
    public string? NewContent { get; init; }
}
