using Microsoft.Extensions.Primitives;

namespace PeopleDirectoryApplication.Clients;

public sealed class AuthCookieForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthCookieForwardingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is not null &&
            context.Request.Headers.TryGetValue("Cookie", out StringValues cookies) &&
            !StringValues.IsNullOrEmpty(cookies) &&
            !request.Headers.Contains("Cookie"))
        {
            request.Headers.TryAddWithoutValidation("Cookie", cookies.ToString());
        }

        return base.SendAsync(request, cancellationToken);
    }
}
