using Microsoft.Extensions.DependencyInjection;
using Work_IA.Application.Agents.Roles;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents;

public sealed class AgentRoleFactory
{
    private static readonly Dictionary<AgentRole, Type> AgentTypes = new()
    {
        [AgentRole.HeadOfEngineering] = typeof(HeadOfEngineeringAgent),
        [AgentRole.TechLeadBackend] = typeof(TechLeadBackendAgent),
        [AgentRole.TechLeadFrontend] = typeof(TechLeadFrontendAgent),
        [AgentRole.TechLeadGame] = typeof(TechLeadGameAgent),
        [AgentRole.TechLeadDevOps] = typeof(TechLeadDevOpsAgent),
        [AgentRole.Architect] = typeof(ArchitectAgent),
        [AgentRole.Specialist] = typeof(SpecialistAgent),
        [AgentRole.TestLead] = typeof(TestLeadAgent),
        [AgentRole.ChiefReviewer] = typeof(ChiefReviewerAgent),
        [AgentRole.AuditLead] = typeof(AuditLeadAgent),
        [AgentRole.Ceo] = typeof(CeoAgent),
    };

    private readonly IServiceProvider _services;
    
    public AgentRoleFactory(IServiceProvider services)
    {
        _services = services;
    }
    
    public IAgent Create(AgentRole role)
    {
        if (!AgentTypes.TryGetValue(role, out var agentType))
            throw new ArgumentException($"Unknown role: {role}");
        
        return (IAgent)ActivatorUtilities.CreateInstance(_services, agentType);
    }
}
