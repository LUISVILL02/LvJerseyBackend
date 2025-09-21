using Authentication.Application.Dtos;
using Authentication.Infrastructure.Abstractions;
using Domain.Entities;
using LvJersey.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Abstractions;

namespace Authentication.Application.Commands.LoginWithSocial;

public class AuthSocialCommandHandler(ApplicationDbContext context,
    IExternalAuthValidator externalValidator, IJwtUtil jwt) : ICommandHandler<AuthSocialCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> HandleAsync(AuthSocialCommand command)
    {
        var userInfo = await externalValidator.ValidateAsync(command.provider, command.tokenId);

        var user = await context.Set<User>()
            .Include(u => u.Role)   
            .FirstOrDefaultAsync(u => u.Email == userInfo.Email);

        if (user is not null) return AuthResponseDto();

        user = new User
        {
            Email = userInfo.Email,
            Nikname = userInfo.Name,
            Role = context.Set<Role>()
                .FirstOrDefault(role => role.Name == Enum.GetName(RoleEnum.User))!
        };
        
        var response = AuthResponseDto();
        
        await context.AddAsync(user);
        await context.SaveChangesAsync();

        return response;
        
        AuthResponseDto AuthResponseDto()
        {
            var accesToken = jwt.GenerateToken(user);
            var refreshToken = jwt.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(12);

            return new AuthResponseDto(accesToken, refreshToken);
        }
    }
}