using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.Organizations.Compatibility;
using WorkFit.TalentManagement.Compatibility;

namespace WorkFit.Host.Compatibility;

public sealed class CompatibilityMiddleware
{
    private readonly RequestDelegate _next;

    public CompatibilityMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.TrimEnd('/') ?? string.Empty;
        var method = context.Request.Method;

        if (HttpMethods.IsGet(method))
        {
            // 1. GET /api/employees -> TalentManagement module service
            if (path.Equals("/api/employees", StringComparison.OrdinalIgnoreCase))
            {
                var userId = GetUserIdFromClaims(context.User);
                var service = context.RequestServices.GetRequiredService<IGetEmployeesCompatService>();
                var employees = await service.GetEmployeesAsync(userId, context.RequestAborted);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await JsonSerializer.SerializeAsync(context.Response.Body, employees, cancellationToken: context.RequestAborted);
                return;
            }

            // 2. GET /api/organizations/me/id -> Organizations module service
            if (path.Equals("/api/organizations/me/id", StringComparison.OrdinalIgnoreCase))
            {
                var userIdStr = context.Request.Query["userId"].FirstOrDefault();
                Guid targetUserId = Guid.Empty;

                if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var parsed))
                {
                    targetUserId = parsed;
                }

                if (targetUserId == Guid.Empty)
                {
                    targetUserId = GetUserIdFromClaims(context.User);
                }

                var service = context.RequestServices.GetRequiredService<IGetOrganizationIdCompatService>();
                var orgId = await service.GetOrganizationIdAsync(targetUserId, context.RequestAborted);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await JsonSerializer.SerializeAsync(context.Response.Body, orgId, cancellationToken: context.RequestAborted);
                return;
            }

            // 3. GET /api/talent-management/employees/user -> TalentManagement module service
            if (path.Equals("/api/talent-management/employees/user", StringComparison.OrdinalIgnoreCase))
            {
                var userId = GetUserIdFromClaims(context.User);
                var service = context.RequestServices.GetRequiredService<IGetEmployeeUserCompatService>();
                var profile = await service.GetEmployeeUserAsync(userId, context.RequestAborted);

                if (profile is null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await JsonSerializer.SerializeAsync(context.Response.Body, profile, cancellationToken: context.RequestAborted);
                return;
            }
        }

        await _next(context);
    }

    private static Guid GetUserIdFromClaims(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst("sub")?.Value
                    ?? user.FindFirst("id")?.Value;

        return !string.IsNullOrEmpty(claim) && Guid.TryParse(claim, out var guid) ? guid : Guid.Empty;
    }
}
