using Shared.Application.Abstractions;

namespace Authentication.Application.Commands.ResendCodeVerificatión;

public record ResendCodeVerificatiónCommand(
    string email
) : ICommand<bool>;