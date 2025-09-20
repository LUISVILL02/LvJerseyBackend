namespace Authentication.Application.Commands.RefreshToken;

public record RefreshTokenCommand(int idUser, string refresh);