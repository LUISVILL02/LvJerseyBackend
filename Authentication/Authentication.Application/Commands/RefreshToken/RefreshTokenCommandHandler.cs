using Authentication.Application.Dtos;
using Authentication.Infrastructure.Abstractions;
using Domain.Entities;
using LvJersey.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Abstractions;

namespace Authentication.Application.Commands.RefreshToken;

public class RefreshTokenCommandHandler(ApplicationDbContext contex, IJwtUtil jwt) : ICommandHandler<RefreshTokenCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> HandleAsync(RefreshTokenCommand command)
    {
        var user = await contex.Set<User>()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.IdUser == command.IdUser);

        if (user is null || user.RefreshTokenExpiryTime < DateTime.UtcNow || user.RefreshToken != command.Refresh) 
            throw new UnauthorizedAccessException("Sin autorización, vuelve a iniciar sesión");

        var accestoken = jwt.GenerateToken(user);
        var refreshToken = jwt.GenerateRefreshToken();
        
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(12);

        contex.Update(user);
        await contex.SaveChangesAsync();
        
        return new AuthResponseDto(accestoken, refreshToken);
    }
}