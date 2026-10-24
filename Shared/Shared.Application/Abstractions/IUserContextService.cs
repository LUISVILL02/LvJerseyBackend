namespace Shared.Application.Abstractions;

public interface IUserContextService
{
    int? GetUserId();
    string? GetUserEmail();
    string? GetClaim(string claimType);
}
