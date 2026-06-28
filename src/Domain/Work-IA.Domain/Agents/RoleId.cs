namespace Work_IA.Domain.Agents;public readonly record struct RoleId(Guid Value){    public static RoleId New() => new(Guid.NewGuid());    public static RoleId From(Guid value) => new(value);}
