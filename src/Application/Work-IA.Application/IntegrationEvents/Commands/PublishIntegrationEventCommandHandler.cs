using MediatR;
using Work_IA.Application.Common.Interfaces;

namespace Work_IA.Application.IntegrationEvents.Commands;

public sealed class PublishIntegrationEventCommandHandler : IRequestHandler<PublishIntegrationEventCommand>
{
    private readonly IEventBus _eventBus;

    public PublishIntegrationEventCommandHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task Handle(PublishIntegrationEventCommand request, CancellationToken cancellationToken)
    {
        var eventType = request.Event.GetType();
        var publishMethod = typeof(IEventBus).GetMethod(nameof(IEventBus.PublishAsync))!
            .MakeGenericMethod(eventType);

        var task = (Task)publishMethod.Invoke(_eventBus, [request.Event, cancellationToken])!;
        await task;
    }
}
