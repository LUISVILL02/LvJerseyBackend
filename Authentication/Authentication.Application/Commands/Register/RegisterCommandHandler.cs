using Authentication.Domain.Exceptions;
using Domain.Entities;
using LvJersey.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Shared.Infrastructure.Abstractions;

namespace Authentication.Application.Commands.Register;

public class RegisterCommandHandler(ApplicationDbContext context) : ICommandHandler<RegisterCommand, bool>
{
    public async Task<bool> HandleAsync(RegisterCommand command)
    {
        var inner = new List<Exception>();

        var user = context.Set<User>().FirstOrDefault(u => u.Email == command.Email);
        if(user is not null) inner.Add(new UserFoundException($"El email {command.Email} ya existe."));
        
        user = context.Set<User>().FirstOrDefault(u => u.Nikname == command.NickName);
        if (user is not null) inner.Add(new UserFoundException($"El nickName {command.NickName} ya está en uso."));
        
        if (inner.Count > 0) throw new AggregateException("", inner);
        
        user = new User
        {
            Email = command.Email,
            Password = command.Password,
            Nikname = command.NickName
        };

        user.Role = context.Set<Role>()
            .FirstOrDefault(role => role.Name == Enum.GetName(RoleEnum.User))!;
        
        var hashedPassword = new PasswordHasher<User>().HashPassword(user, command.Password);
        user.Password = hashedPassword;
        
        context.Set<User>().Add(user);
        await context.SaveChangesAsync();

        return true;
    }
}