namespace Work_IA.Infrastructure.Persistence;

public sealed class RoleEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string TechnologiesJson { get; set; } = "[]";
    public string MethodologiesJson { get; set; } = "[]";
    public string ToolsJson { get; set; } = "[]";
}
