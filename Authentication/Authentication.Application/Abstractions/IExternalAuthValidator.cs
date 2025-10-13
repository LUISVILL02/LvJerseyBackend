using Authentication.Application.Dtos;

namespace Authentication.Application.Abstractions;

public interface IExternalAuthValidator
{
    Task<ExternalUserInfo> ValidateAsync(string provider, string tokenId);
}