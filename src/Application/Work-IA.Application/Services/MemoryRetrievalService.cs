using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;
using Work_IA.Domain.Memory;

namespace Work_IA.Application.Services;

public sealed class MemoryRetrievalService
{
    private readonly IMemoryStore _memoryStore;
    
    public MemoryRetrievalService(IMemoryStore memoryStore)
    {
        _memoryStore = memoryStore;
    }
    
    public async Task<IReadOnlyList<MemoryEntry>> FindRelevantAsync(string context, int maxResults = 5)
    {
        return await _memoryStore.SearchAsync(context, maxResults);
    }
    
    public async Task<IReadOnlyList<MemoryEntry>> FindFailuresAsync(string? errorContext = null, int maxResults = 5)
    {
        var failures = await _memoryStore.GetByTypeAsync(MemoryType.Failure, maxResults);
        
        if (!string.IsNullOrEmpty(errorContext))
        {
            var contextual = await _memoryStore.SearchAsync(errorContext, maxResults);
            failures = failures.Concat(contextual)
                .Where(m => m.Type == MemoryType.Failure)
                .Distinct()
                .Take(maxResults)
                .ToList();
        }
        
        return failures;
    }
    
    public async Task<IReadOnlyList<MemoryEntry>> GetBestPracticesAsync(string? domain = null, int maxResults = 10)
    {
        var practices = await _memoryStore.GetByTypeAsync(MemoryType.BestPractice, maxResults);
        
        if (!string.IsNullOrEmpty(domain))
        {
            practices = practices.Where(m =>
                m.Tags.Contains(domain, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }
        
        return practices;
    }
    
    public async Task RecordSuccessAsync(string title, string content, AgentId? agentId = null, List<string>? tags = null)
    {
        var entry = MemoryEntry.Create(MemoryType.Success, title, content, tags, 10, agentId);
        await _memoryStore.SaveAsync(entry);
    }
    
    public async Task RecordFailureAsync(string title, string content, AgentId? agentId = null, List<string>? tags = null)
    {
        var entry = MemoryEntry.Create(MemoryType.Failure, title, content, tags, 1, agentId);
        await _memoryStore.SaveAsync(entry);
    }
    
    public async Task RecordLessonAsync(string title, string content, AgentId? agentId = null, List<string>? tags = null)
    {
        var entry = MemoryEntry.Create(MemoryType.Lesson, title, content, tags, 8, agentId);
        await _memoryStore.SaveAsync(entry);
    }
}
