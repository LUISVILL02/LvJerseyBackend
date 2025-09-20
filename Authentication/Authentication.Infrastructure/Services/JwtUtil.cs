using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Authentication.Infrastructure.Abstractions;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Infrastructure.Services;

public class JwtUtil(IConfiguration configuration) : IJwtUtil
{
    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim("idUser", user.IdUser.ToString()),
            new Claim("email", user.Email),
            new Claim("rol", user.Role.Name!),
            new Claim("nikName", user.Nikname!),
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