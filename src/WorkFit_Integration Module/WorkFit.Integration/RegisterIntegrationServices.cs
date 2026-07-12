using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.Integration.Contracts.Abstractions;
using WorkFit.Integration.Infrastructure.Data;
using WorkFit.Integration.Providers.Jira;
using WorkFit.Integration.Services;
using WorkFit.SharedKernel.RegisterModuleServices;

namespace WorkFit.Integration;

/// <summary>
/// Registers all services needed for the Integration module.
/// Auto-discovered by host assembly scanning — no changes to Program.cs required.
///
/// To swap the provider (e.g. GitHub instead of Jira):
///   1. Implement IProjectManagementProvider in a new class.
///   2. Change the registration below from JiraProjectManagementProvider to your class.
///   3. No other changes needed.
/// </summary>
public sealed class RegisterIntegrationServices : IRegisterModuleServices
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // ── Integration DbContext ─────────────────────────────────────────────
        services.AddDbContext<IntegrationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // ── Named HTTP client for Jira ────────────────────────────────────────
        services.AddHttpClient("Jira", client =>
        {
            // BaseUrl is no longer static — it's set per-request from org settings.
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        // ── Jira provider infrastructure ──────────────────────────────────────
        services.AddScoped<JiraApiClient>();
        services.AddScoped<IProjectManagementProvider, JiraProjectManagementProvider>();

        // ── Orchestration service ─────────────────────────────────────────────
        services.AddScoped<IIntegrationSyncService, IntegrationSyncService>();
    }
}
