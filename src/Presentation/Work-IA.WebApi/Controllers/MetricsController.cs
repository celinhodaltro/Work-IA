using Microsoft.AspNetCore.Mvc;
using Work_IA.Application.Services;

namespace Work_IA.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class MetricsController : ControllerBase
{
    private readonly MetricsService _metrics;
    
    public MetricsController(MetricsService metrics)
    {
        _metrics = metrics;
    }
    
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_metrics.GetSnapshot());
    }
}
