using Work_IA.Domain.Agents;

namespace Work_IA.Application.Configuration;

public sealed class RoleDefinitionProvider
{
    private static readonly Dictionary<AgentCareerLevel, RoleDefinition> Definitions = new()
    {
        [AgentCareerLevel.Intern] = new RoleDefinition("Intern", AgentCareerLevel.Intern, ["read"], 0),
        [AgentCareerLevel.Junior] = new RoleDefinition("Junior", AgentCareerLevel.Junior, ["read", "write"], 100),
        [AgentCareerLevel.Pleno] = new RoleDefinition("Pleno", AgentCareerLevel.Pleno, ["read", "write", "review"], 500),
        [AgentCareerLevel.Senior] = new RoleDefinition("Senior", AgentCareerLevel.Senior, ["read", "write", "review", "approve"], 1000),
        [AgentCareerLevel.TechLead] = new RoleDefinition("Tech Lead", AgentCareerLevel.TechLead, ["read", "write", "review", "approve", "delegate"], 3000),
        [AgentCareerLevel.Architect] = new RoleDefinition("Architect", AgentCareerLevel.Architect, ["read", "write", "review", "approve", "delegate", "design"], 6000),
        [AgentCareerLevel.Head] = new RoleDefinition("Head", AgentCareerLevel.Head, ["all"], 10000),
    };

    public RoleDefinition? GetByLevel(AgentCareerLevel level)
    {
        return Definitions.GetValueOrDefault(level);
    }
}
