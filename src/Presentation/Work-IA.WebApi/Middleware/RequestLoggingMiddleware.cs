using System.Diagnostics;

namespace Work_IA.WebApi.Middleware;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path;
        
        await _next(context);
        
        sw.Stop();
        
        _logger.LogInformation(
            "{Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
            method, path, context.Response.StatusCode, sw.ElapsedMilliseconds);
    }
}
