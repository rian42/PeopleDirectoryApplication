using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PeopleDirectoryApplication.Application.Contracts.Services;

namespace PeopleDirectoryApplication.Infrastructure.Services;

public class HttpCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentUserIdentifier()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return "system";
        }

        return user.FindFirstValue(ClaimTypes.Email)
               ?? user.Identity?.Name
               ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? "authenticated-user";
    }
}
