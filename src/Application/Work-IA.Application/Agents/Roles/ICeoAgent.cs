using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Roles;

public interface ICeoAgent
{
    Task ReceiveWeeklySummaryAsync(string summary);
}
