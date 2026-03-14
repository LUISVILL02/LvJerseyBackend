using Authentication.Application.Dtos;
using Shared.Application.Abstractions;

namespace Authentication.Application.Commands.Login;

public record LoginCommand(string UserName, string Password) : ICommand<AuthResponseDto>;