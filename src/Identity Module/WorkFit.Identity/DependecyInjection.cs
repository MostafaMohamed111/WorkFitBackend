
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.Identity.Domain.Entities;
using WorkFit.Identity.Infrastructure.Data;

namespace WorkFit.Identity;

public static class DependecyInjection
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<WorkFitUsersDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddIdentityCore<WorkFitUser>()
                .AddRoles<WorkFitRole>()
                .AddEntityFrameworkStores<WorkFitUsersDbContext>();

        return services;
    }

}
