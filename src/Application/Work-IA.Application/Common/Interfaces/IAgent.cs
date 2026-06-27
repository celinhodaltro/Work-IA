using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Common.Interfaces;

public interface IAgent
{
    AgentId AgentId { get; }
    string Name { get; }
    AgentRole Role { get; }
    AgentStatus Status { get; }
    
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task StartAsync(CancellationToken cancellationToken = default);
    Task PauseAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    
    Task HandleEventAsync(IDomainEvent @event, CancellationToken cancellationToken = default);
    Task HandleMessageAsync(AgentMessage message, CancellationToken cancellationToken = default);
    Task<TaskResult> ExecuteTaskAsync(AgentTask task, CancellationToken cancellationToken = default);
}
