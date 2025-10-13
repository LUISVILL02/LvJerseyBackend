using Authentication.Application.Abstractions;
using Authentication.Application.Abstractions.Users;
using Authentication.Domain.Exceptions;
using Shared.Application.Abstractions;

namespace Authentication.Application.Commands.ResendCodeVerificatión;

public class ResendCodeVerificatiónCommandHandler(IUserAuthRepository userRepo, IVerificationService verification) : ICommandHandler<ResendCodeVerificatiónCommand, bool>
{
    public async Task<bool> HandleAsync(ResendCodeVerificatiónCommand command)
    {
        var user = await userRepo.GetByUserNameAsync(command.email);

        if (user is null) throw new UserFoundException("usuario no encontrado");
        
        var code = await verification.SendEmailVerificationCodeAsync(user.Nickname!, user.Email);
        return await userRepo.UpdateCodeVerificationInfoAsync(user.IdUser, code);
    }
}