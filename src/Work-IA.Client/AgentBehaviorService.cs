using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Agents;
using Work_IA.Domain.Agents;
using Work_IA.Domain.Events;
using Work_IA.Client.Rendering;

namespace Work_IA.Client;

public sealed class AgentBehaviorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AgentBehaviorService> _logger;
    private static readonly Random _random = new();

    public AgentBehaviorService(IServiceScopeFactory scopeFactory, ILogger<AgentBehaviorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Agent Behavior Engine started");
        await Task.Delay(2000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var registry = scope.ServiceProvider.GetRequiredService<AgentRegistry>();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                foreach (var ia in registry.GetAll())
                {
                    var agent = ia is AgentBase ab ? ab.Agent : null;
                    if (agent is null || agent.Status != AgentStatus.Running) continue;

                    AgentAction newAction = agent.CurrentAction;

                    if (agent.ActionTimer > 25 && agent.CurrentAction == AgentAction.Working)
                        newAction = _random.NextDouble() < 0.3 ? AgentAction.Resting : AgentAction.Working;
                    else if (agent.ActionTimer > 10 && agent.CurrentAction == AgentAction.Resting)
                        newAction = AgentAction.Working;
                    else if (agent.ActionTimer > 12 && agent.CurrentAction == AgentAction.Chatting)
                        newAction = AgentAction.Idle;
                    else if (agent.ActionTimer > 4 && agent.CurrentAction == AgentAction.Idle)
                    {
                        var roll = _random.NextDouble();
                        newAction = roll < 0.5 ? AgentAction.Working : roll < 0.8 ? AgentAction.Chatting : AgentAction.Resting;
                    }

                    if (newAction != agent.CurrentAction)
                    {
                        agent.CurrentAction = newAction;
                        agent.ActionTimer = 0;
                        agent.Emotion = newAction switch
                        {
                            AgentAction.Working => AgentEmotion.Focused,
                            AgentAction.Chatting => AgentEmotion.Happy,
                            AgentAction.Resting => AgentEmotion.Relaxed,
                            _ => AgentEmotion.Neutral
                        };
                        agent.Position = new AgentPosition(
                            agent.Position.X + (float)(_random.NextDouble() * 200 - 100),
                            agent.Position.Y + (float)(_random.NextDouble() * 200 - 100));

                        if (newAction == AgentAction.Chatting)
                        {
                            var candidates = registry.GetAll().Where(a => a.AgentId != agent.AgentId && a.Status == AgentStatus.Running).ToList();
                            if (candidates.Count > 0)
                            {
                                var target = candidates[_random.Next(candidates.Count)];
                                agent.ConversationTopic = new[] { "arquitetura", "código", "performance", "testes" }[_random.Next(4)];
                                agent.ConversationPartner = target.AgentId;
                            }
                        }
                        else
                        {
                            agent.ConversationTopic = null;
                            agent.ConversationPartner = null;
                        }

                        await mediator.Publish(new AgentStateChangedEvent(
                            agent.AgentId, agent.Name.Value, agent.Title?.Value ?? "",
                            agent.Status.ToString(), agent.Emotion.ToString(), agent.CurrentAction.ToString(),
                            agent.ConversationTopic, agent.Position.X, agent.Position.Y
                        ), stoppingToken);
                    }

                    agent.ActionTimer += 2;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Behavior Engine error");
            }

            await Task.Delay(3000, stoppingToken);
        }
    }
}
