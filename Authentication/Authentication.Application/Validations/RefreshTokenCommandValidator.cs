using Authentication.Application.Commands.RefreshToken;
using FluentValidation;

namespace Authentication.Application.Validations;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(command => command.refresh)
            .NotNull()
            .WithMessage("El refresh token no puede ser null")
            .NotEmpty()
            .WithMessage("El refresh token no puede estar vacio");
    }
}