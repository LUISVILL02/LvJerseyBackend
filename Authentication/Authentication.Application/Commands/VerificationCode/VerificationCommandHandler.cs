using Authentication.Application.Abstractions.Users;
using Shared.Application.Abstractions;

namespace Authentication.Application.Commands.VerificationCode;

public class VerificationCommandHandler(IUserAuthRepository userRepo) : ICommandHandler<VerificationCommand, bool>
{
    public async Task<bool> HandleAsync(VerificationCommand command)
    {
        var user = await userRepo.GetByUserNameAsync(command.Email);

        if (user is null) return false;

        return await userRepo.GetEmailVerificationInfoAsync(user.IdUser, command.Code);
    }
}