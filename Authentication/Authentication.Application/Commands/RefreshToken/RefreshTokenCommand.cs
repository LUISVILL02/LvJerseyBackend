using Authentication.Application.Dtos;
using Shared.Application.Abstractions;

namespace Authentication.Application.Commands.RefreshToken;

public record RefreshTokenCommand(int IdUser, string Refresh) : ICommand<AuthResponseDto>;