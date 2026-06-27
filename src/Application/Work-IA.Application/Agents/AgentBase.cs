using MediatR;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents;

public abstract class AgentBase : IAgent
{
    protected readonly Agent Agent;
    protected readonly IEventBus EventBus;
    protected readonly IMediator Mediator;
    protected readonly ILogger Logger;
    
    public AgentId AgentId => Agent.Id;
    public string Name => Agent.Name.Value;
    public AgentRole Role => Agent.Role;
    public AgentStatus Status => Agent.Status;
    
    protected AgentBase(Agent agent, IEventBus eventBus, IMediator mediator, ILogger logger)
    {
        Agent = agent;
        EventBus = eventBus;
        Mediator = mediator;
        Logger = logger;
    }

    protected AgentBase(string name, AgentRole role, IEventBus eventBus, IMediator mediator, ILogger logger)
        : this(Agent.Create(new AgentName(name), role), eventBus, mediator, logger)
    {
    }
    
    protected void RegisterObservation(string eventType, ObservationPriority priority = ObservationPriority.Medium, Func<object, bool>? condition = null)
    {
        Agent.AddObservationRule(new ObservationRule(eventType, priority, condition));
    }
    
    public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (Agent.Status == AgentStatus.Created)
        {
            await Agent.StartAsync();
            await DispatchDomainEventsAsync(cancellationToken);
        }
        
        Logger.LogInformation("Agent {AgentName} initialized and running", Name);
    }
    
    public virtual async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await Agent.StartAsync();
        await DispatchDomainEventsAsync(cancellationToken);
    }
    
    public virtual async Task PauseAsync(CancellationToken cancellationToken = default)
    {
        await Agent.PauseAsync();
        await DispatchDomainEventsAsync(cancellationToken);
    }
    
    public virtual async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await Agent.StopAsync();
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
    
    protected abstract Task<TaskResult> ProcessTaskAsync(AgentTask task, CancellationToken cancellationToken);
    
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
