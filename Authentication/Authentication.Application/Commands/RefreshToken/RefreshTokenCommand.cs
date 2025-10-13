namespace Authentication.Application.Commands.RefreshToken;

public record RefreshTokenCommand(int IdUser, string Refresh);