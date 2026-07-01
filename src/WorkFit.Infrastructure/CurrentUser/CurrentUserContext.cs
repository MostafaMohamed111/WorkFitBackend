
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using WorkFit.SharedKernel.ICurrentUser;

namespace WorkFit.Infrastructure.CurrentUser;

public sealed class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public Guid GetUserId(CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            throw new InvalidOperationException("User ID not found.");
        return Guid.Parse(userId);
    }
}
