using MediatR;
using Work_IA.Application.Common.Interfaces;

namespace Work_IA.Application.IntegrationEvents.Commands;

public sealed record PublishIntegrationEventCommand(IIntegrationEvent Event) : IRequest;
