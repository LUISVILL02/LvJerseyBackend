using Authentication.Application.Abstractions;
using Authentication.Application.Dtos;
using Authentication.Infrastructure.Constants;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace Authentication.Infrastructure.Services;

public class ExternalAuthValidator : IExternalAuthValidator
{
    private readonly string _googleClientId;
    public ExternalAuthValidator(IConfiguration config)
    {
        _googleClientId = config["Authentication:Google:ClientId"]
            ?? throw new InvalidOperationException("Authentication:Google:ClientId is not configured");
    }
    public async Task<ExternalUserInfo> ValidateAsync(string provider, string tokenId)
    {
        if (provider.Equals(ProvidersAuthConstans.GoogleProvider, StringComparison.OrdinalIgnoreCase))
        {
            var userPayload = await GoogleJsonWebSignature.ValidateAsync(tokenId, 
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _googleClientId }
                });
            return new ExternalUserInfo
            (
                userPayload.Subject,
                userPayload.Email,
                userPayload.Name
            );
        }
        throw new NotSupportedException($"Provider '{provider}' is not supported");
    }
}