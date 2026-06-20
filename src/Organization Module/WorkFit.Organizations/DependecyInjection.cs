
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.DependencyInjection;

namespace WorkFit.Organizations;

public static class DependecyInjection
{
    public static IServiceCollection AddOrganizationModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrganizationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register Mediator handlers for the Organization module
        services.AddMediatorHandlers<ModuleMarker>();

        return services;
    }
}
