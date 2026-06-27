using Work_IA.Domain.Abstractions;

namespace Work_IA.Domain.Agents;

public sealed class TokenUsage : Entity<Guid>
{
    public AgentId AgentId { get; private set; }
    public string ModelName { get; private set; }
    public int TokensIn { get; private set; }
    public int TokensOut { get; private set; }
    public int TotalTokens => TokensIn + TokensOut;
    public decimal Cost { get; private set; }
    public string TaskDescription { get; private set; }
    public DateTime Timestamp { get; private set; }

    private static readonly Dictionary<string, (decimal In, decimal Out)> ModelCosts = new()
    {
        ["claude-3-opus"] = (0.015m, 0.075m),
        ["claude-3-sonnet"] = (0.003m, 0.015m),
        ["claude-3-haiku"] = (0.00025m, 0.00125m),
        ["gpt-4"] = (0.03m, 0.06m),
        ["gpt-4-turbo"] = (0.01m, 0.03m),
        ["gpt-3.5-turbo"] = (0.0005m, 0.0015m),
        ["opencode-default"] = (0.003m, 0.015m),
    };

    private TokenUsage() : base(Guid.NewGuid()) { }

    public static TokenUsage Record(AgentId agentId, string model, int tokensIn, int tokensOut, string task)
    {
        var (costIn, costOut) = ModelCosts.TryGetValue(model.ToLowerInvariant(), out var c) ? c : (0.005m, 0.02m);

        return new TokenUsage
        {
            AgentId = agentId,
            ModelName = model,
            TokensIn = tokensIn,
            TokensOut = tokensOut,
            Cost = (tokensIn / 1000m * costIn) + (tokensOut / 1000m * costOut),
            TaskDescription = task,
            Timestamp = DateTime.UtcNow
        };
    }
}
