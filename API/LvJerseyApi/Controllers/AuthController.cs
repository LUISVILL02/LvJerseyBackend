using Authentication.Application.Commands.Login;
using Authentication.Application.Commands.LoginWithSocial;
using Authentication.Application.Commands.RefreshToken;
using Authentication.Application.Commands.Register;
using Authentication.Application.Commands.ResendCodeVerificatión;
using Authentication.Application.Commands.VerificationCode;
using Authentication.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Abstractions;

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

    [HttpPost, Route("auth-social")]
    public async Task<IActionResult> AuthSocial([FromBody] AuthSocialCommand authSocial)
    {
        var response = await sender.SendCommandAsync<AuthSocialCommand, AuthResponseDto>(authSocial);
        return Ok(response);
    }

    [HttpPost, Route("verified-code")]
    public async Task<IActionResult> VerfiedCode([FromBody] VerificationCommand code)
    {
        var response = await sender.SendCommandAsync<VerificationCommand, bool>(code);
        return response ?  Ok(response) : BadRequest("No se pudo verificar el email, Intente de nuevo");
    }

    [HttpPost, Route("resend-code-verification")]
    public async Task<IActionResult> ResendVerifiedCodeVerification(
        [FromBody] ResendCodeVerificatiónCommand verification)
    {
        var response = await sender.SendCommandAsync<ResendCodeVerificatiónCommand, bool>(verification);
        return response ?  Ok(response) : BadRequest("No se pudo volver a enviar el código, Vuelve a registrarte");
    }
    
}