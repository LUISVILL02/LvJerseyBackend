using Authentication.Application.Abstractions;
using Shared.Application.Abstractions;

namespace Authentication.Infrastructure.Services;

public class VerificationService(IEmailSender emailSender) : IVerificationService
{
    public async Task<string> SendEmailVerificationCodeAsync(string nickname, string email)
    {
        var code = GenerateVerificationCode();
        var subject = "Código de verificación LVJersey";
        var body = $"Hola {nickname}, tu código es: {code}";
        await emailSender.SendEmailAsync(email, subject, body);
        return code;
    }
    
    private static string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}