using Authentication.Application.Commands.ResendCodeVerificatión;
using FluentValidation;

namespace Authentication.Application.Validations;

public class ResendCodeVerificatiónCommandValidator : AbstractValidator<ResendCodeVerificatiónCommand>
{
    public ResendCodeVerificatiónCommandValidator()
    {
        RuleFor(resendCommand => resendCommand.email)
            .NotEmpty()
            .WithMessage("Email es requerido");

    }
}