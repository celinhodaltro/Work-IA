using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Roles;

namespace Work_IA.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly WorkIaDbContext _context;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public RoleRepository(WorkIaDbContext context) => _context = context;

    public async Task<RoleDefinition?> GetByIdAsync(RoleId id, CancellationToken ct = default)
    {
        var e = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id.Value, ct);
        return e is null ? null : MapToDomain(e);
    }

    public async Task<List<RoleDefinition>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await _context.Roles.ToListAsync(ct);
        return entities.Select(MapToDomain).ToList();
    }

    public async Task AddAsync(RoleDefinition role, CancellationToken ct = default)
    {
        _context.Roles.Add(new RoleEntity
        {
            Id = role.RoleId.Value,
            Name = role.Name,
            TechnologiesJson = JsonSerializer.Serialize(role.Technologies, JsonOpts),
            MethodologiesJson = JsonSerializer.Serialize(role.Methodologies, JsonOpts),
            ToolsJson = JsonSerializer.Serialize(role.Tools, JsonOpts)
        });

        if (role.DomainEvents.Count > 0)
        {
            _context.EnqueueDomainEvents(role.DomainEvents);
            role.ClearDomainEvents();
        }
    }

    public async Task UpdateAsync(RoleDefinition role, CancellationToken ct = default)
    {
        var e = await _context.Roles.FirstAsync(r => r.Id == role.RoleId.Value, ct);
        e.Name = role.Name;
        e.TechnologiesJson = JsonSerializer.Serialize(role.Technologies, JsonOpts);
        e.MethodologiesJson = JsonSerializer.Serialize(role.Methodologies, JsonOpts);
        e.ToolsJson = JsonSerializer.Serialize(role.Tools, JsonOpts);

        if (role.DomainEvents.Count > 0)
        {
            _context.EnqueueDomainEvents(role.DomainEvents);
            role.ClearDomainEvents();
        }
    }

    private static RoleDefinition MapToDomain(RoleEntity e)
    {
        return RoleDefinition.Create(
            e.Name,
            JsonSerializer.Deserialize<List<string>>(e.TechnologiesJson, JsonOpts) ?? [],
            JsonSerializer.Deserialize<List<string>>(e.MethodologiesJson, JsonOpts) ?? [],
            JsonSerializer.Deserialize<List<string>>(e.ToolsJson, JsonOpts) ?? []);
    }
}
