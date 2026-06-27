namespace Work_IA.Domain.Agents;

public sealed class AgentCostSummary
{
    public AgentId AgentId { get; set; }
    public string AgentName { get; set; } = "";
    public string AgentTitle { get; set; } = "";
    public int TotalTokensIn { get; set; }
    public int TotalTokensOut { get; set; }
    public int TotalTokens => TotalTokensIn + TotalTokensOut;
    public decimal TotalCost { get; set; }
    public int TaskCount { get; set; }
    public string MostUsedModel { get; set; } = "";
}
