using Microsoft.AspNetCore.Mvc;

namespace LvJerseyApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Helper");
    }
}