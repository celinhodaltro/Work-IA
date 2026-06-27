using MediatR;
using Work_IA.Application.Behaviors;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Commands;

public sealed record RecordTokenUsageCommand(
    AgentId AgentId,
    string Model,
    int TokensIn,
    int TokensOut,
    string Task) : IRequest, IRequiresUnitOfWork;
