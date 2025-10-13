using Authentication.Application.Abstractions;
using Authentication.Application.Abstractions.Users;
using Authentication.Application.Dtos;
using Shared.Application.Abstractions;

namespace Authentication.Application.Commands.LoginWithSocial;

public class AuthSocialCommandHandler(IUserAuthRepository userRepo,
    IExternalAuthValidator externalValidator, IJwtUtil jwt) : ICommandHandler<AuthSocialCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> HandleAsync(AuthSocialCommand command)
    {
        var userInfo = await externalValidator.ValidateAsync(command.provider, command.tokenId);

        var user = await userRepo.GetByUserNameAsync(userInfo.Email);

        if (user is not null)
        {
            var jwtUserinfo = new JwtUserInfo
                (
                    user.IdUser,
                    user.Email,
                    user.Role,
                    user.Nickname!
                );
            return await AuthResponseDto(jwtUserinfo, user.IdUser);
        }

        var userJwt = await userRepo.SaveUserFromSocialAsync(userInfo.Email, userInfo.Name);
        
        return await AuthResponseDto(userJwt!, userJwt!.IdUser);
    }

    private async Task<AuthResponseDto> AuthResponseDto(JwtUserInfo jwtUserinfo, int idUser)
    {
        var accesToken = jwt.GenerateToken(jwtUserinfo);
        var refreshToken = jwt.GenerateRefreshToken();
        await userRepo.UpdateRefreshAndTimeToken(idUser, refreshToken);
        return new AuthResponseDto(accesToken, refreshToken);
    }
}