namespace Authentication.Application.Dtos;

public record JwtUserInfo(
    int IdUser,
    string Email,
    string Role,
    string Nickname
);
