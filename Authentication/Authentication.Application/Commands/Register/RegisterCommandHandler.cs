using Authentication.Application.Abstractions;
using Authentication.Application.Abstractions.Users;
using Authentication.Domain.Exceptions;
using Shared.Application.Abstractions;

namespace Authentication.Application.Commands.Register;

public class RegisterCommandHandler(IUserAuthRepository userRepo, IVerificationService verification) : ICommandHandler<RegisterCommand, bool>
{
    public async Task<bool> HandleAsync(RegisterCommand command)
    {
        var inner = new List<Exception>();

        var user = await userRepo.GetByUserNameAsync(command.Email);
        if(user is not null && user.EmailConfirmed) inner.Add(new UserFoundException($"El email {command.Email} ya existe."));
        
        user = await userRepo.GetByUserNameAsync(command.NickName);
        if (user is not null && user.EmailConfirmed) inner.Add(new UserFoundException($"El nickName {command.NickName} ya está en uso."));
        
        if (inner.Count > 0) throw new AggregateException("", inner);

        var idUser = 0;
        
        if (user is null)
        {
            idUser = await userRepo.RegisterUserAsync(command);
        }
        else
        {
            idUser = user.IdUser;
        }

        var code = await verification.SendEmailVerificationCodeAsync(command.NickName, command.Email);
        await userRepo.AggreCodeExpirationAsync(idUser, code);

        return true;
    }
}