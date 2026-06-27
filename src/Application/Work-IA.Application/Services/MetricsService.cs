using System.Collections.Concurrent;

namespace Work_IA.Application.Services;

public sealed class MetricsService
{
    private long _eventsProcessed;
    private long _tasksCompleted;
    private long _tasksFailed;
    private readonly ConcurrentDictionary<string, long> _eventsByType = new();
    private readonly ConcurrentQueue<double> _latencySamples = new();
    private readonly DateTime _startTime = DateTime.UtcNow;
    
    public void RecordEventProcessed(string eventType, double latencyMs)
    {
        Interlocked.Increment(ref _eventsProcessed);
        _eventsByType.AddOrUpdate(eventType, 1, (_, count) => count + 1);
        _latencySamples.Enqueue(latencyMs);
        if (_latencySamples.Count > 1000)
            _latencySamples.TryDequeue(out _);
    }
    
    public void RecordTaskCompleted()
    {
        Interlocked.Increment(ref _tasksCompleted);
    }
    
    public void RecordTaskFailed()
    {
        Interlocked.Increment(ref _tasksFailed);
    }
    
    public MetricsSnapshot GetSnapshot()
    {
        var latencies = _latencySamples.ToArray();
        var sorted = latencies.OrderBy(x => x).ToArray();
        var p50 = sorted.Length > 0 ? sorted[sorted.Length / 2] : 0;
        var p95 = sorted.Length > 0 ? sorted[(int)(sorted.Length * 0.95)] : 0;
        var p99 = sorted.Length > 0 ? sorted[(int)(sorted.Length * 0.99)] : 0;
        
        return new MetricsSnapshot
        {
            EventsProcessed = _eventsProcessed,
            TasksCompleted = _tasksCompleted,
            TasksFailed = _tasksFailed,
            SuccessRate = _tasksCompleted + _tasksFailed > 0
                ? (double)_tasksCompleted / (_tasksCompleted + _tasksFailed) * 100 : 100,
            LatencyP50Ms = p50,
            LatencyP95Ms = p95,
            LatencyP99Ms = p99,
            UptimeSeconds = (int)(DateTime.UtcNow - _startTime).TotalSeconds,
            EventsByType = _eventsByType.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };
    }
}

public sealed class MetricsSnapshot
{
    public long EventsProcessed { get; set; }
    public long TasksCompleted { get; set; }
    public long TasksFailed { get; set; }
    public double SuccessRate { get; set; }
    public double LatencyP50Ms { get; set; }
    public double LatencyP95Ms { get; set; }
    public double LatencyP99Ms { get; set; }
    public int UptimeSeconds { get; set; }
    public Dictionary<string, long> EventsByType { get; set; } = [];
}
