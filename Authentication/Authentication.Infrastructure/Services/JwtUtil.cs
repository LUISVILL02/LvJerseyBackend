using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Authentication.Application.Abstractions;
using Authentication.Application.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Infrastructure.Services;

public class JwtUtil(IConfiguration configuration) : IJwtUtil
{
    public string GenerateToken(JwtUserInfo user)
    {
        var claims = new[]
        {
            new Claim("idUser", user.IdUser.ToString()),
            new Claim("email", user.Email),
            new Claim("rol", user.Role!.ToUpper()),
            new Claim("nikName", user.Nickname!),
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public string GenerateRefreshToken()
    {
        var numeroAleatorio = new byte[32];
        using (var generador = RandomNumberGenerator.Create())
        {
            generador.GetBytes(numeroAleatorio);
            return Convert.ToBase64String(numeroAleatorio);
        }
    }
}