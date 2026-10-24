using Microsoft.AspNetCore.Mvc;

namespace LvJerseyApi.Controllers;

[ApiController]
[Route("api/v0.0.1/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
        });
    }

    [HttpGet("live")]
    public IActionResult Live()
    {
        return Ok(new { status = "Live" });
    }

    [HttpGet("ready")]
    public IActionResult Ready()
    {
        return Ok(new { status = "Ready", timestamp = DateTime.UtcNow });
    }
}
