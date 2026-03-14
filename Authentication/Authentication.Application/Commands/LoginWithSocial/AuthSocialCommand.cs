using Authentication.Application.Dtos;
using Shared.Application.Abstractions;

namespace Authentication.Application.Commands.LoginWithSocial;

public record AuthSocialCommand(string tokenId, string provider) : ICommand<AuthResponseDto>;