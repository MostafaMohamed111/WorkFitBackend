
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.Identity.AuthServices.Jwt;
using WorkFit.Identity.Contracts.IdentityServices;
using WorkFit.Identity.CrossModule.RegisterOrganization;
using WorkFit.Identity.Domain.Entities;
using WorkFit.Identity.Infrastructure.Data;
using WorkFit.SharedKernel.DependencyInjection;
using WorkFit.SharedKernel.RegisterModuleServices;

namespace WorkFit.Identity;

public sealed class RegisterIdentityModuleServices : IRegisterModuleServices
{
    public  void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<WorkFitUsersDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddIdentityCore<WorkFitUser>()
                .AddRoles<WorkFitRole>()
                .AddEntityFrameworkStores<WorkFitUsersDbContext>();

        services.AddMediatorHandlers<ModuleMarker>();
        services.AddScoped<JwtTokenGenerator>();
        services.AddScoped<ICreateOrganizationUserService, RegisterOrganizationCommandHandler>();
    }

}
