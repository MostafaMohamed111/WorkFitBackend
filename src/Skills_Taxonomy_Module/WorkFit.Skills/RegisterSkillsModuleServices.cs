using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.SharedKernel.RegisterModuleServices;
using WorkFit.Skills.Contracts.SkillLookUp;
using WorkFit.Skills.Infrastructure.Data;

namespace WorkFit.Skills;

public sealed class RegisterSkillsModuleServices : IRegisterModuleServices
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<WorkFitSkillsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(WorkFitSkillsDbContext).Assembly.FullName)
            )
        );

        // ⭐ Remove this if it's causing issues - MediatR is now registered in Host
        // services.AddMediatorHandlers<ModuleMarker>();

        // Register services
        services.AddScoped<ISkillLookUpService, SkillLookUpService>();
    }

    void IRegisterModuleServices.RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        Register(services, configuration);
    }
}