namespace Authentication.Application.Dtos;

public record AuthResponseDto(string AccessToken, string RefreshToken);