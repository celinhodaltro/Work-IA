using MediatR;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents;

public class AgentBase : IAgent
{
    protected readonly Agent Agent;
    protected readonly IEventBus EventBus;
    protected readonly IMediator Mediator;
    protected readonly ILogger Logger;
    
    public AgentId AgentId => Agent.AgentId;
    public string Name => Agent.Name.Value;
    public AgentTitle Title => Agent.Title;
    public AgentCareerLevel CareerLevel => Agent.CareerLevel;
    public AgentStatus Status => Agent.Status;
    public IReadOnlyList<AgentSkill> Skills => Agent.Skills.AsReadOnly();
    
    public AgentBase(Agent agent, IEventBus eventBus, IMediator mediator, ILogger logger)
    {
        Agent = agent;
        EventBus = eventBus;
        Mediator = mediator;
        Logger = logger;
    }

    protected void RegisterObservation(string eventType, ObservationPriority priority = ObservationPriority.Medium, Func<object, bool>? condition = null)
    {
        Agent.AddObservationRule(new ObservationRule(eventType, priority, condition));
    }
    
    public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (Agent.Status == AgentStatus.Created)
        {
            Agent.Start();
            await DispatchDomainEventsAsync(cancellationToken);
        }
        
        Logger.LogInformation("Agent {AgentName} initialized and running", Name);
    }
    
    public virtual async Task StartAsync(CancellationToken cancellationToken = default)
    {
        Agent.Start();
        await DispatchDomainEventsAsync(cancellationToken);
    }
    
    public virtual async Task PauseAsync(CancellationToken cancellationToken = default)
    {
        Agent.Pause();
        await DispatchDomainEventsAsync(cancellationToken);
    }
    
    public virtual async Task StopAsync(CancellationToken cancellationToken = default)
    {
        Agent.Stop();
        await DispatchDomainEventsAsync(cancellationToken);
    }
    
    public virtual Task HandleEventAsync(IDomainEvent @event, CancellationToken cancellationToken = default)
    {
        var relevant = Agent.ObservationRules.Any(r =>
            r.EventType == @event.EventType &&
            (r.Condition is null || r.Condition(@event)));
        
        if (relevant)
        {
            Logger.LogInformation("Agent {AgentName} observed event {EventType}", Name, @event.EventType);
        }
        
        return Task.CompletedTask;
    }
    
    public virtual Task HandleMessageAsync(AgentMessage message, CancellationToken cancellationToken = default)
    {
        Agent.ReceiveMessage(message);
        Logger.LogInformation("Agent {AgentName} received message from {FromId}", Name, message.From);
        return Task.CompletedTask;
    }
    
    public virtual async Task<TaskResult> ExecuteTaskAsync(AgentTask task, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Agent {AgentName} executing task {TaskTitle}", Name, task.Title);
        
        task.Start();
        
        try
        {
            var result = await ProcessTaskAsync(task, cancellationToken);
            task.Complete();
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Agent {AgentName} failed task {TaskTitle}", Name, task.Title);
            task.Fail();
            return TaskResult.Fail(ex.Message);
        }
    }
    
    protected virtual Task<TaskResult> ProcessTaskAsync(AgentTask task, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Agent {AgentName} processing: {Task}", Name, task.Title);
        return Task.FromResult(TaskResult.Ok($"Task completed by {Name}: {task.Title}"));
    }
    
    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var domainEvents = Agent.DomainEvents.ToList();
        Agent.ClearDomainEvents();
        
        foreach (var domainEvent in domainEvents)
        {
            await Mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
