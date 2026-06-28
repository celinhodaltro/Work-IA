using Work_IA.Domain.Agents;

namespace Work_IA.Application.Services;

public sealed class BehaviorDecider
{
    private static readonly Random _random = new();

    public AgentAction Decide(Agent agent, AgentAction currentAction, float busyTimeSeconds)
    {
        if (currentAction == AgentAction.Working && busyTimeSeconds > 30)
        {
            if (_random.NextDouble() < 0.2)
                return AgentAction.Resting;
            if (_random.NextDouble() < 0.15)
                return AgentAction.Chatting;
            return AgentAction.Working;
        }

        if (currentAction == AgentAction.Resting && busyTimeSeconds > 10)
            return AgentAction.Working;

        if (currentAction == AgentAction.Chatting && busyTimeSeconds > 15)
            return _random.NextDouble() < 0.5 ? AgentAction.Working : AgentAction.Idle;

        if (currentAction == AgentAction.InMeeting && busyTimeSeconds > 45)
            return AgentAction.Working;

        if (currentAction == AgentAction.Idle && busyTimeSeconds > 5)
        {
            var roll = _random.NextDouble();
            if (roll < 0.45) return AgentAction.Working;
            if (roll < 0.25) return AgentAction.Chatting;
            if (roll < 0.15) return AgentAction.Resting;
            if (roll < 0.10) return AgentAction.Thinking;
            return AgentAction.Idle;
        }

        return currentAction;
    }

    public AgentEmotion GetEmotionForAction(AgentAction action)
    {
        return action switch
        {
            AgentAction.Working => AgentEmotion.Focused,
            AgentAction.Chatting => AgentEmotion.Happy,
            AgentAction.InMeeting => AgentEmotion.Thinking,
            AgentAction.Resting => AgentEmotion.Relaxed,
            AgentAction.Celebrating => AgentEmotion.Excited,
            AgentAction.Thinking => AgentEmotion.Curious,
            _ => AgentEmotion.Neutral
        };
    }

    public (AgentId target, string topic)? PickConversationPartner(Agent agent, List<Agent> allAgents)
    {
        var candidates = allAgents
            .Where(a => a.AgentId != agent.AgentId && a.Status == AgentStatus.Running)
            .ToList();

        if (candidates.Count == 0)
            return null;

        var target = candidates[_random.Next(candidates.Count)];
        var topics = new[] { "arquitetura", "código", "performance", "testes", "deploy", "tecnologias", "carreira" };
        var topic = topics[_random.Next(topics.Length)];
        return (target.AgentId, topic);
    }
}
