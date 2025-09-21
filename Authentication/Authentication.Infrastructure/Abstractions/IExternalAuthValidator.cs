using Authentication.Infrastructure.Utils;
using Authentication.Infrastructure.Utils.Dtos;

namespace Authentication.Infrastructure.Abstractions;

public interface IExternalAuthValidator
{
    Task<ExternalUserInfo> ValidateAsync(string provider, string tokenId);
}