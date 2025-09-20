using Authentication.Application.Commands;
using Authentication.Application.Commands.Login;
using Authentication.Application.Commands.RefreshToken;
using Authentication.Application.Commands.Register;
using Authentication.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Shared.Infrastructure.Abstractions;

namespace LvJerseyApi.Controllers;

[ApiController]
[Route("api/v0.0.1/[controller]")]
public class AuthController(ISender sender) : ControllerBase
{
    [HttpPost, Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand register)
    {
        await sender.SendCommandAsync<RegisterCommand, bool>(register);
        return Created();
    }

    [HttpPost, Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand login)
    {
        var response = await sender.SendCommandAsync<LoginCommand, AuthResponseDto>(login);
        return Ok(response);
    }

    [HttpPost, Route("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand refresh)
    {
        var response = await sender.SendCommandAsync<RefreshTokenCommand, AuthResponseDto>(refresh);
        return Ok(response);
    }
}