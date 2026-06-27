using System.Collections.Concurrent;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents;

public sealed class AgentRegistry
{
    private readonly ConcurrentDictionary<AgentId, IAgent> _agents = new();
    
    public void Register(IAgent agent)
    {
        _agents[agent.AgentId] = agent;
    }
    
    public void Unregister(AgentId agentId)
    {
        _agents.TryRemove(agentId, out _);
    }
    
    public IAgent? Get(AgentId agentId)
    {
        _agents.TryGetValue(agentId, out var agent);
        return agent;
    }
    
    public IReadOnlyList<IAgent> GetAll()
    {
        return _agents.Values.ToList();
    }
    
    public IReadOnlyList<IAgent> GetByCareerLevel(AgentCareerLevel level)
    {
        return _agents.Values.Where(a => a.CareerLevel == level).ToList();
    }

    public IReadOnlyList<IAgent> GetByTitle(string title)
    {
        return _agents.Values.Where(a => a.Title.Value.Equals(title, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public IReadOnlyList<IAgent> GetBySkill(string skillName, int minProficiency = 1)
    {
        return _agents.Values.Where(a => a.Skills.Any(s => 
            s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase) && 
            s.Proficiency >= minProficiency)).ToList();
    }
    
    public IReadOnlyList<IAgent> GetOnline()
    {
        return _agents.Values.Where(a => a.Status == AgentStatus.Running).ToList();
    }
    
    public int Count => _agents.Count;
}
