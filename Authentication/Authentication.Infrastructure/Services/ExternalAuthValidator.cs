using Authentication.Infrastructure.Abstractions;
using Authentication.Infrastructure.Utils;
using Authentication.Infrastructure.Utils.Dtos;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace Authentication.Infrastructure.Services;

public class ExternalAuthValidator : IExternalAuthValidator
{
    private readonly string _googleClientId;
    public ExternalAuthValidator(IConfiguration config)
    {
        _googleClientId = config["Authentication:Google:ClientId"];
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
            {
                ProviderId = userPayload.Subject,
                Email = userPayload.Email,
                Name = userPayload.Name
            };
        }
        return null;
    }
}