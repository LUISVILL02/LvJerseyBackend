using Authentication.Application.Dtos;

namespace Authentication.Application.Abstractions;

public interface IJwtUtil
{
    string GenerateToken(JwtUserInfo user);
    string GenerateRefreshToken();
}