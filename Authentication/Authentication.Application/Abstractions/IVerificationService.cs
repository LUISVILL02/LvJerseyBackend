using Authentication.Application.Dtos.Users;

namespace Authentication.Application.Abstractions;

public interface IVerificationService
{
    Task<string> SendEmailVerificationCodeAsync(string nickname, string email);
}