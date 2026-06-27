using MediatR;
using Work_IA.Application.Behaviors;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Commands;

public sealed record AddExperienceCommand(AgentId AgentId, int Points) : IRequest, IRequiresUnitOfWork;
