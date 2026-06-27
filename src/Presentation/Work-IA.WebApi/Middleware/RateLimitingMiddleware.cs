using System.Collections.Concurrent;

namespace Work_IA.WebApi.Middleware;

public sealed class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ConcurrentDictionary<string, RateLimitEntry> _clients = new();
    private readonly int _maxRequests = 100;
    private readonly TimeSpan _window = TimeSpan.FromMinutes(1);
    
    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var entry = _clients.GetOrAdd(clientIp, _ => new RateLimitEntry());
        
        lock (entry)
        {
            entry.Requests.RemoveAll(r => r < DateTime.UtcNow - _window);
            
            if (entry.Requests.Count >= _maxRequests)
            {
                context.Response.StatusCode = 429;
                context.Response.Headers["Retry-After"] = "60";
                return;
            }
            
            entry.Requests.Add(DateTime.UtcNow);
        }
        
        await _next(context);
    }
    
    private sealed class RateLimitEntry
    {
        public List<DateTime> Requests { get; } = new();
    }
}
