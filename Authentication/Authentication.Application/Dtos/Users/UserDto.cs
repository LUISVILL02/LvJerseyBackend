namespace Authentication.Application.Dtos.Users;

public record UserDto(
    int IdUser, 
    string Email,
    string? Names,
    string? LastName,
    string? Nickname,
    string Role,
    string? RefreshToken,
    string? Password,
    DateTime RefreshTokenExpiryTime,
    bool EmailConfirmed
);