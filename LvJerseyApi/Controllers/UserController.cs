using Microsoft.AspNetCore.Mvc;
using Shared.Infrastructure.Abstractions;
using Users.Application.Commands;

namespace LvJerseyApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> UpdateInfoUser([FromBody] UpdateInfoCommand updateInfo)
    {
        await sender.SendAsync(updateInfo);
        return Ok();
    }

    /*[HttpGet]
    public async Task<IActionResult> GetUserInfo()
    {
        await sender.SendAsync < new { message = "Get User Info" } > ();
    }*/
}