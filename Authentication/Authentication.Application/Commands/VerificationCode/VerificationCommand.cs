namespace Authentication.Application.Commands.VerificationCode;

public record VerificationCommand(
    string Code,
    string Email
    );