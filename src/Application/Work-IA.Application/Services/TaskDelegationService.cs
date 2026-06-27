using Work_IA.Application.Agents;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Services;

public sealed class TaskDelegationService
{
    private readonly AgentRegistry _registry;
    private readonly IEventBus _eventBus;
    
    public TaskDelegationService(AgentRegistry registry, IEventBus eventBus)
    {
        _registry = registry;
        _eventBus = eventBus;
    }
    
    public async Task<TaskResult> DelegateAsync(string title, string description, AgentRole targetRole, TaskPriority priority = TaskPriority.Normal)
    {
        var candidates = _registry.GetByRole(targetRole)
            .Where(a => a.Status == AgentStatus.Running)
            .ToList();
        
        if (candidates.Count == 0)
        {
            return TaskResult.Fail($"No available agents found for role {targetRole}");
        }
        
        var target = candidates[Random.Shared.Next(candidates.Count)];
        
        var task = AgentTask.Create(title, description, target.AgentId, null, priority);
        
        return await target.ExecuteTaskAsync(task);
    }
    
    public async Task<TaskResult> DelegateToSpecificAsync(AgentId agentId, string title, string description, TaskPriority priority = TaskPriority.Normal)
    {
        var agent = _registry.Get(agentId);
        
        if (agent is null)
        {
            return TaskResult.Fail($"Agent {agentId} not found");
        }
        
        var task = AgentTask.Create(title, description, agentId, null, priority);
        
        return await agent.ExecuteTaskAsync(task);
    }
    
    public IReadOnlyList<IAgent> FindCandidatesForTask(string taskDescription, AgentRole preferredRole)
    {
        return _registry.GetByRole(preferredRole)
            .Where(a => a.Status == AgentStatus.Running)
            .ToList();
    }
}
