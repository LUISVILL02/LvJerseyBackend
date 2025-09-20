namespace Authentication.Application.Commands.Register;

public record RegisterCommand(
    string NickName,
    string Email,
    string Password
);