using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Memory;
using Work_IA.Infrastructure.Persistence;

namespace Work_IA.Infrastructure.Memory;

public sealed class DatabaseMemoryStore : IMemoryStore
{
    private readonly WorkIaDbContext _context;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public DatabaseMemoryStore(WorkIaDbContext context)
    {
        _context = context;
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }
    
    public async Task SaveAsync(MemoryEntry entry, CancellationToken cancellationToken = default)
    {
        var entity = new MemoryEntryEntity
        {
            Id = Guid.NewGuid(),
            MemoryId = entry.Id.Value,
            Type = (int)entry.Type,
            Title = entry.Title,
            Content = entry.Content,
            TagsJson = JsonSerializer.Serialize(entry.Tags, _jsonOptions),
            Score = entry.Score,
            AgentId = entry.AgentId?.Value,
            CreatedAt = entry.CreatedAt,
            UpdatedAt = entry.UpdatedAt
        };
        
        await _context.MemoryEntries.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<MemoryEntry?> GetByIdAsync(MemoryId memoryId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.MemoryEntries
            .FirstOrDefaultAsync(e => e.MemoryId == memoryId.Value, cancellationToken);
        
        return entity is null ? null : MapToDomain(entity);
    }
    
    public async Task<IReadOnlyList<MemoryEntry>> SearchAsync(string query, int limit = 10, CancellationToken cancellationToken = default)
    {
        var entities = await _context.MemoryEntries
            .Where(e => e.Title.Contains(query) || e.Content.Contains(query))
            .OrderByDescending(e => e.Score)
            .ThenByDescending(e => e.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
        
        return entities.Select(MapToDomain).ToList();
    }
    
    public async Task<IReadOnlyList<MemoryEntry>> GetByTypeAsync(MemoryType type, int limit = 10, CancellationToken cancellationToken = default)
    {
        var entities = await _context.MemoryEntries
            .Where(e => e.Type == (int)type)
            .OrderByDescending(e => e.Score)
            .ThenByDescending(e => e.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
        
        return entities.Select(MapToDomain).ToList();
    }
    
    public async Task<IReadOnlyList<MemoryEntry>> GetByTagsAsync(string[] tags, int limit = 10, CancellationToken cancellationToken = default)
    {
        var entities = await _context.MemoryEntries
            .ToListAsync(cancellationToken);
        
        var matched = entities
            .Where(e =>
            {
                var entryTags = JsonSerializer.Deserialize<List<string>>(e.TagsJson, _jsonOptions) ?? [];
                return tags.Any(t => entryTags.Contains(t));
            })
            .OrderByDescending(e => e.Score)
            .Take(limit)
            .Select(MapToDomain)
            .ToList();
        
        return matched;
    }
    
    public async Task DeleteAsync(MemoryId memoryId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.MemoryEntries
            .FirstOrDefaultAsync(e => e.MemoryId == memoryId.Value, cancellationToken);
        
        if (entity is not null)
        {
            _context.MemoryEntries.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
    
    private static MemoryEntry MapToDomain(MemoryEntryEntity entity)
    {
        var tags = string.IsNullOrEmpty(entity.TagsJson)
            ? []
            : (System.Text.Json.JsonSerializer.Deserialize<List<string>>(entity.TagsJson) ?? []);
        
        var memoryId = MemoryId.From(entity.MemoryId);
        var agentId = entity.AgentId.HasValue ? (Domain.Agents.AgentId?)new Domain.Agents.AgentId(entity.AgentId.Value) : null;
        
        var entry = MemoryEntry.Create(
            (MemoryType)entity.Type,
            entity.Title,
            entity.Content,
            tags,
            entity.Score,
            agentId);
        
        return entry;
    }
}
