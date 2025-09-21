using Authentication.Application.Dtos;
using Authentication.Domain.Exceptions;
using Authentication.Infrastructure.Abstractions;
using Domain.Entities;
using LvJersey.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Abstractions;

namespace Authentication.Application.Commands.Login;

public class LoginCommandHandler(ApplicationDbContext context, IJwtUtil jwt) : ICommandHandler<LoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> HandleAsync(LoginCommand command)
    {
        var user = await context.Set<User>()
            .Where(user => user.Email == command.UserName ||
                           user.Nikname == command.UserName)
            .Include(user => user.Role)
            .SingleOrDefaultAsync();

        if (user is null || user.Password is null) throw new UserFoundException("Credenciales inválidos");
        
        var passwordHash = new PasswordHasher<User>()
            .VerifyHashedPassword(user, user.Password!, command.Password);
        
        if(passwordHash == PasswordVerificationResult.Failed) throw new UserFoundException("Credenciales inválidos");

        
        var token = jwt.GenerateToken(user);
        var refreshToken = jwt.GenerateRefreshToken();
        
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(12);

        context.Update(user);
        await context.SaveChangesAsync();
        
        return new AuthResponseDto(token, refreshToken);
    }
}