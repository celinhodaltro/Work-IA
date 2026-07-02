using MediatR;

namespace Work_IA.Application.Agents.Commands;

public sealed record ChatWithAgentCommand(Guid AgentId, string Message, List<ChatMessageItem>? History = null) : IRequest<string>;

public sealed record ChatMessageItem(string Sender, string Text);
