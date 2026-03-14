using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shared.Application.Abstractions;

namespace Shared.Infrastructure.Services;

public sealed class HttpContextUserContextService(IHttpContextAccessor _httpContextAccessor) : IUserContextService
{
    public int? GetUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            return null;
        }

        var userIdClaim = user.FindFirst("idUser");
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
    }

    public ClaimsPrincipal? GetUser()
        => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated
        => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public string? GetClaim(string claimType)
        => _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
}

