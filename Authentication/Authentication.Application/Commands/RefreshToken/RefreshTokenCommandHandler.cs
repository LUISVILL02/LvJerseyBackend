using Authentication.Application.Abstractions;
using Authentication.Application.Abstractions.Users;
using Authentication.Application.Dtos;
using Shared.Application.Abstractions;

namespace Authentication.Application.Commands.RefreshToken;

public class RefreshTokenCommandHandler(IUserAuthRepository userRepo, IJwtUtil jwt) : ICommandHandler<RefreshTokenCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> HandleAsync(RefreshTokenCommand command)
    {
        var user = await userRepo.GetByIdAsync(command.IdUser);

        if (user is null || user.RefreshTokenExpiryTime < DateTime.UtcNow || user.RefreshToken != command.Refresh) 
            throw new UnauthorizedAccessException("Sin autorización, vuelve a iniciar sesión");

        var jwtUserInfo = new JwtUserInfo(
            user.IdUser,
            user.Email,
            user.Role,
            user.Nickname!
        );
        var accestoken = jwt.GenerateToken(jwtUserInfo);
        var refreshToken = jwt.GenerateRefreshToken();
        
        await userRepo.UpdateRefreshAndTimeToken(user.IdUser, refreshToken);
        
        return new AuthResponseDto(accestoken, refreshToken);
    }
}