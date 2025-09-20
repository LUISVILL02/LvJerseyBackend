using Authentication.Application.Commands;
using Authentication.Application.Commands.Register;
using FluentValidation;
using FluentValidation.Validators;

namespace Authentication.Application.Validations;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(command => command.NickName)
            .NotEmpty()
            .WithMessage("El nombre de usuario es requerido");
        RuleFor(command => command.Email)
            .NotEmpty()
            .WithMessage("El correo es requerido")
            .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible);
        RuleFor(command => command.Password)
            .NotEmpty()
            .WithMessage("La contraseña es requerida");
    }
}