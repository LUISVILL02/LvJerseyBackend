using Authentication.Application.Abstractions.Users;
using Authentication.Application.Commands.Register;
using Authentication.Application.Dtos;
using Authentication.Application.Dtos.Users;
using Authentication.Domain.Exceptions;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using Shared.Infrastructure.Data;

namespace Users.Infrastructure.Services;

public class UserReadService(ApplicationDbContext context) : IUserAuthRepository
{
    public async Task<UserDto?> GetByUserNameAsync(string userName)
    {
        var user = await context.Set<User>()
            .Include(u => u.Role)
            .Include(a => a.Adresses)
            .SingleOrDefaultAsync(u => u.Email == userName || u.Nickname == userName);

        return user is not null
            ? new UserDto(
                user.IdUser,
                user.Email, 
                user.Names,                
                user.LastName,             
                user.Nickname,             
                user.Role.Name!,            
                user.RefreshToken,    
                user.Password!,
                user.RefreshTokenExpiryTime,
                user.EmailConfirmed      
            )
            : null;
    }

    public async Task<UserDto?> GetByIdAsync(int userId)
    {
        var user = await context.Set<User>()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.IdUser == userId);

        return user is not null ? new UserDto(
            user.IdUser,
            user.Email,
            user.Names,
            user.LastName,
            user.Nickname,
            user.Role.Name!,
            user.RefreshToken,
            user.Password!,
            user.RefreshTokenExpiryTime,
            user.EmailConfirmed
        ) : null;
    }

    public async Task<UserDto?> UpdateRefreshAndTimeToken(int idUser, string refreshToken)
    {
        var user = await context.Set<User>()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.IdUser == idUser);

        if (user is null) return null;
        
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(12);

        context.Update(user);
        await context.SaveChangesAsync();

        return new UserDto(
            user.IdUser,
            user.Email,
            user.Names,
            user.LastName,
            user.Nickname,
            user.Role.Name!,
            user.RefreshToken,
            user.Password!,
            user.RefreshTokenExpiryTime,
            user.EmailConfirmed
        );
    }

    public async Task<JwtUserInfo?> SaveUserFromSocialAsync(string email, string nickname)
    {

        var userEntity = new User
        {
            Email = email,
            Nickname = nickname,
            Role = (await context.Set<Role>()
                .FirstOrDefaultAsync(r => r.Name == RoleEnum.User.ToString()))!,
            EmailConfirmed = true
        };
        
        await context.AddAsync(userEntity);
        await context.SaveChangesAsync();
        return new JwtUserInfo(
            userEntity.IdUser,
            userEntity.Email,
            userEntity.Nickname,
            userEntity.Role.Name!
        );
    }

    public async Task<int> RegisterUserAsync(RegisterCommand userCommand)
    {

        var user = new User
        {
            Email = userCommand.Email,
            Nickname = userCommand.NickName,
            Role = context.Set<Role>()
                .FirstOrDefault(role => role.Name == Enum.GetName(RoleEnum.User))!
        };

        var hashedPassword = new PasswordHasher<User>().HashPassword(user, userCommand.Password);
        user.Password = hashedPassword;
        
        context.Set<User>().Add(user);
        await context.SaveChangesAsync();

        return user.IdUser;
    }

    public async Task AggreCodeExpirationAsync(int idUser, string code)
    {
        var user = await context.Set<User>()
            .FindAsync(idUser);

        if (user is null) throw new NotFoundException("El usuario no existe");

        var emailVerification = new EmailVerification
        {
            Code = code,
            ExpiryAt = DateTime.UtcNow.AddMinutes(5),
            User = user
        };
        
        context.Set<EmailVerification>().Add(emailVerification);
        await context.SaveChangesAsync();
    }

    public async  Task<bool> GetEmailVerificationInfoAsync(int idUser, string code)
    {
        var emailVerification = await context.Set<EmailVerification>()
            .FirstOrDefaultAsync(e => e.Code == code && e.UserId == idUser && e.ExpiryAt >= DateTime.UtcNow);

        if (emailVerification is null) return false;
        
        var user = await  context.Set<User>()
            .Include(u => 
                u.EmailVerifications
                    .Where(e => e.Code == code && e.UserId == idUser))
            .FirstOrDefaultAsync(u => u.IdUser == idUser);

        if(user is null) throw new NotFoundException("El usuario no existe");
        
        context.Set<EmailVerification>().Remove(emailVerification);
        user.EmailConfirmed = true;
        
        context.Update(user);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateCodeVerificationInfoAsync(int idUser, string code)
    {
        var user = await context.Set<User>().FirstOrDefaultAsync(u => u.IdUser == idUser);
        if (user is null) throw new UserFoundException("El usuario no existe");
        
        var userCodeVerification = await context.Set<EmailVerification>()
                                    .FirstOrDefaultAsync(e => e.UserId == idUser);

        if(userCodeVerification is null)
        {
            var emailverification = new EmailVerification
            {
                Code = code,
                ExpiryAt = DateTime.UtcNow.AddMinutes(5),
                User = user
            };
            context.Set<EmailVerification>().Add(emailverification);
            await context.SaveChangesAsync();
            return true;
        }
        else
        {
            userCodeVerification.Code = code;
            userCodeVerification.ExpiryAt = DateTime.UtcNow.AddMinutes(5);
            context.Update(userCodeVerification);
            await context.SaveChangesAsync();
            return true;
        }
    }
}