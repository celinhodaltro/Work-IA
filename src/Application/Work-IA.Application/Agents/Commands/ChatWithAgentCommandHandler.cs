using MediatR;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Services;
using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Commands;

public sealed class ChatWithAgentCommandHandler : IRequestHandler<ChatWithAgentCommand, string>
{
    private readonly IAgentRepository _agents;
    private readonly IOpenCodeService _openCode;
    private readonly ILogger<ChatWithAgentCommandHandler> _logger;

    public ChatWithAgentCommandHandler(
        IAgentRepository agents,
        IOpenCodeService openCode,
        ILogger<ChatWithAgentCommandHandler> logger)
    {
        _agents = agents;
        _openCode = openCode;
        _logger = logger;
    }

    public async Task<string> Handle(ChatWithAgentCommand request, CancellationToken ct)
    {
        var agent = await _agents.GetByIdAsync(new AgentId(request.AgentId), ct);
        if (agent is null)
            return "Agent not found.";

        var history = request.History ?? [];
        var context = string.Join("\n", history.Select(h => $"{h.Sender}: {h.Text}"));
        var levelName = agent.CareerLevel.ToString();

        var prompt = $"""
You are {agent.Name.Value}, a {levelName} level {agent.Title.Value} at AI Office OS.
Your personality and skills: you are a professional AI agent with expertise in your role.
Respond to the user's message in character, as {agent.Name}. Be helpful and concise.

Conversation history:
{context}
User: {request.Message}

{agent.Name}:
""";

        var result = await _openCode.ExecutePromptAsync(
            new OpenCodePrompt(prompt, new Dictionary<string, string>()), ct);

        if (!result.Success)
        {
            _logger.LogWarning("OpenCode chat failed: {Error}", result.Error);
            return "I'm having trouble processing that right now. Could you try again?";
        }

        return result.ExtractedJson ?? result.RawOutput;
    }
}
