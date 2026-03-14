using Jerseys.Application.Queries.JerseysHome;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Abstractions;

namespace LvJerseyApi.Controllers;

[ApiController]
[Route("api/v0.0.1/[controller]")]
public class JerseyController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetJerseyshome()
    {
        var result = await sender.SendQueryAsync<HomeJerseysQuery, List<LeagueWithJerseysResponse>>(new HomeJerseysQuery());
        return Ok(result);
    }
}