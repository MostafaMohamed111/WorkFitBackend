
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.Identity.AuthServices.Jwt;
using WorkFit.Identity.Contracts.IdentityServices;
using WorkFit.Identity.CrossModule.RegisterEmployee;
using WorkFit.Identity.CrossModule.RegisterOrganization;
using WorkFit.Identity.Domain.Entities;
using WorkFit.Identity.Infrastructure.Data;
using WorkFit.Identity.Infrastructure.Email;
using WorkFit.SharedKernel.DependencyInjection;
using WorkFit.SharedKernel.RegisterModuleServices;
using WorkFit.Identity.CrossModule;

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

        services.AddOptions<IdentityEmailOptions>()
            .Bind(configuration.GetSection(IdentityEmailOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.FrontendBaseUrl),
                "Identity frontend base URL is required.")
            .ValidateOnStart();

        services.AddScoped<ICreateOrganizationUserService, RegisterOrganizationCommandHandler>();
        services.AddScoped<IEmployeeAccountProvisioningService, EmployeeAccountProvisioningService>();
        services.AddScoped<ICreateEmployeeUserService, RegisterEmployeeUserService>();
    }

}
