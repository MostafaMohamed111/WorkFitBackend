
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.Organizations.Contracts.OrganizationServices;
using WorkFit.Organizations.CrossModule.CreateOrganization;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.DependencyInjection;
using WorkFit.SharedKernel.RegisterModuleServices;

namespace WorkFit.Organizations;

public sealed class RegisterOrganizationModuleServices : IRegisterModuleServices
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrganizationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register Mediator handlers for the Organization module
        services.AddMediatorHandlers<ModuleMarker>();
        services.AddScoped<ICreateOrganizationService, CreateOrganizationCommandHandler>();
    }
}
