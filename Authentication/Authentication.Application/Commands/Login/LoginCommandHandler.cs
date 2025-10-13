using Authentication.Application.Abstractions;
using Authentication.Application.Abstractions.Users;
using Authentication.Application.Dtos;
using Authentication.Application.Dtos.Users;
using Authentication.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using SendGrid.Helpers.Errors.Model;
using Shared.Application.Abstractions;

namespace Authentication.Application.Commands.Login;

public class LoginCommandHandler(IJwtUtil jwt, IUserAuthRepository userRepo) : ICommandHandler<LoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> HandleAsync(LoginCommand command)
    {
        var user = await userRepo.GetByUserNameAsync(command.UserName);

        if (user is null || user.Password is null) throw new UserFoundException("Credenciales inválidos");
        
        var passwordHash = new PasswordHasher<UserDto>()
            .VerifyHashedPassword(user, user.Password!, command.Password);
        
        if(passwordHash == PasswordVerificationResult.Failed) throw new UserFoundException("Credenciales inválidos");

        if (!user.EmailConfirmed) throw new BadRequestException("Coreo no confirmado");
        
        var userJwtInfo = new JwtUserInfo
        (
            user.IdUser,
            user.Email,
            user.Nickname!,
            user.Role
        );
        var token = jwt.GenerateToken(userJwtInfo);
        var refreshToken = jwt.GenerateRefreshToken();
        
        await userRepo.UpdateRefreshAndTimeToken(user.IdUser, refreshToken);
        
        return new AuthResponseDto(token, refreshToken);
    }
}