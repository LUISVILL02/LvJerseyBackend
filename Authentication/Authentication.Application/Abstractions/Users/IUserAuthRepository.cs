using Authentication.Application.Commands.Register;
using Authentication.Application.Dtos;
using Authentication.Application.Dtos.Users;

namespace Authentication.Application.Abstractions.Users;

public interface IUserAuthRepository
{
    Task<UserDto?> GetByUserNameAsync(string userName);
    Task<UserDto?> GetByIdAsync(int userId);
    Task<UserDto?> UpdateRefreshAndTimeToken(int idUser, string refreshToken);
    Task<JwtUserInfo?> SaveUserFromSocialAsync(string email, string nickname);
    Task<int> RegisterUserAsync(RegisterCommand user);
    
    Task AggreCodeExpirationAsync(int idUser, string code);
    
    Task<bool> GetEmailVerificationInfoAsync(int idUser, string code);
    
    Task<bool> UpdateCodeVerificationInfoAsync(int idUser, string code);
}