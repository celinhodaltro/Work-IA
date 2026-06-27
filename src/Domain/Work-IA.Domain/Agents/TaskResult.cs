using Work_IA.Domain.Abstractions;

namespace Work_IA.Domain.Agents;

public sealed class TaskResult : ValueObject
{
    public bool Success { get; }
    public string? Output { get; }
    public string? Error { get; }
    public TimeSpan Duration { get; }
    
    public TaskResult(bool success, string? output = null, string? error = null, TimeSpan? duration = null)
    {
        Success = success;
        Output = output;
        Error = error;
        Duration = duration ?? TimeSpan.Zero;
    }
    
    public static TaskResult Ok(string? output = null)
    {
        return new TaskResult(true, output);
    }
    
    public static TaskResult Fail(string error)
    {
        return new TaskResult(false, null, error);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Success;
        yield return Output ?? string.Empty;
        yield return Error ?? string.Empty;
        yield return Duration;
    }
}
