using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Work_IA.Domain.Agents;

namespace Work_IA.Infrastructure.Configuration;

public sealed class RoleDefinitionProvider
{
    private readonly List<RoleDefinition> _roles;

    public RoleDefinitionProvider(IConfiguration configuration)
    {
        var section = configuration.GetSection("RoleDefinitions");
        if (section.Exists())
        {
            var roles = section.Get<RoleDefinitionConfig[]>();
            _roles = roles?.Select(r => new RoleDefinition(r.Title, (AgentCareerLevel)r.Level, r.Permissions ?? [], r.XpRequired)).ToList() ?? [];
        }
        else
        {
            _roles = GetDefaultRoles();
        }
    }

    public RoleDefinition? GetByLevel(AgentCareerLevel level)
    {
        return _roles.FirstOrDefault(r => r.Level == level);
    }

    public RoleDefinition? GetByTitle(string title)
    {
        return _roles.FirstOrDefault(r => r.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyList<RoleDefinition> GetAll()
    {
        return _roles.AsReadOnly();
    }

    private static List<RoleDefinition> GetDefaultRoles()
    {
        return
        [
            new RoleDefinition("Intern", AgentCareerLevel.Intern, ["observe"], 0),
            new RoleDefinition("Junior Specialist", AgentCareerLevel.Junior, ["observe", "read", "write"], 100),
            new RoleDefinition("Pleno Specialist", AgentCareerLevel.Pleno, ["observe", "read", "write", "delegate"], 500),
            new RoleDefinition("Senior Specialist", AgentCareerLevel.Senior, ["observe", "read", "write", "delegate", "review"], 2000),
            new RoleDefinition("Tech Lead", AgentCareerLevel.TechLead, ["observe", "read", "write", "delegate", "review", "block"], 5000),
            new RoleDefinition("Architect", AgentCareerLevel.Architect, ["*"], 10000),
            new RoleDefinition("Head of Engineering", AgentCareerLevel.Head, ["*"], 20000),
        ];
    }

    private sealed class RoleDefinitionConfig
    {
        public string Title { get; set; } = string.Empty;
        public int Level { get; set; }
        public List<string>? Permissions { get; set; }
        public int XpRequired { get; set; }
    }
}
