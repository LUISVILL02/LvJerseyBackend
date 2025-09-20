using Domain.Entities;

namespace Authentication.Infrastructure.Abstractions;

public interface IJwtUtil
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
}