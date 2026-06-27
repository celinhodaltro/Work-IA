using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;
using Work_IA.Domain.Memory;

namespace Work_IA.Application.Services;

public sealed class LearningEngine
{
    private readonly IMemoryStore _memoryStore;
    private readonly ILogger<LearningEngine> _logger;
    
    public LearningEngine(IMemoryStore memoryStore, ILogger<LearningEngine> logger)
    {
        _memoryStore = memoryStore;
        _logger = logger;
    }
    
    public async Task LearnFromOutcomeAsync(bool success, string title, string detail, AgentId? agentId = null, List<string>? tags = null)
    {
        if (success)
        {
            var entry = MemoryEntry.Create(MemoryType.Success, title, detail, tags, 10, agentId);
            await _memoryStore.SaveAsync(entry);
            _logger.LogInformation("Learned from success: {Title}", title);
        }
        else
        {
            var entry = MemoryEntry.Create(MemoryType.Failure, title, detail, tags, 1, agentId);
            await _memoryStore.SaveAsync(entry);
            _logger.LogWarning("Learned from failure: {Title}", title);
        }
    }
    
    public async Task<MemoryEntry[]> GetActiveRecommendationsAsync(int maxResults = 5)
    {
        var bestPractices = await _memoryStore.GetByTypeAsync(MemoryType.BestPractice, maxResults);
        return bestPractices.ToArray();
    }
    
    public async Task IdentifyRecurringFailuresAsync(int minOccurrences = 3)
    {
        var failures = await _memoryStore.GetByTypeAsync(MemoryType.Failure, 100);
        
        var patterns = failures
            .GroupBy(f => string.Join(",", f.Tags.OrderBy(t => t)))
            .Where(g => g.Count() >= minOccurrences)
            .ToList();
        
        foreach (var pattern in patterns)
        {
            var antiPattern = MemoryEntry.Create(
                MemoryType.AntiPattern,
                $"Recurring pattern: {pattern.Key}",
                $"This failure pattern occurred {pattern.Count()} times",
                pattern.Key.Split(',').ToList(),
                3);
            
            await _memoryStore.SaveAsync(antiPattern);
        }
    }
}
