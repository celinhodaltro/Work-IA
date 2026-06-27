namespace Work_IA.Application.Common.Models;

public sealed class OperationResult
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }

    private OperationResult() { }

    public static OperationResult Success() => new() { IsSuccess = true };
    
    public static OperationResult Failure(string error) => new() { IsSuccess = false, Error = error };
}

public sealed class OperationResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }

    private OperationResult() { }

    public static OperationResult<T> Success(T data) => new() { IsSuccess = true, Data = data };
    
    public static OperationResult<T> Failure(string error) => new() { IsSuccess = false, Error = error };
}
