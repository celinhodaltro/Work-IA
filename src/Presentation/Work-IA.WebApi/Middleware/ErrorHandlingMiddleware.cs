using System.Text.Json;

namespace Work_IA.WebApi.Middleware;

public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    
    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception processing {Path}", context.Request.Path);
            
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            
            var response = new { error = "An internal error occurred", type = ex.GetType().Name };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
