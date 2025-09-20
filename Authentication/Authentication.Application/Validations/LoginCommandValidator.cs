using Authentication.Application.Commands.Login;
using FluentValidation;

namespace Authentication.Application.Validations;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.UserName)
            .NotEmpty()
            .WithName("El nombre de usuario es requerido");
    }
}