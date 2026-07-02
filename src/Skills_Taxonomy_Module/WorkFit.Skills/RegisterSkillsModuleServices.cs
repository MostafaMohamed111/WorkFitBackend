using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.SharedKernel.DependencyInjection;
using WorkFit.SharedKernel.RegisterModuleServices;
using WorkFit.Skills.Contracts.SkillLookUp;
using WorkFit.Skills.Infrastructure.Data;

namespace WorkFit.Skills;

public sealed class RegisterSkillsModuleServices : IRegisterModuleServices
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<WorkFitSkillsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(WorkFitSkillsDbContext).Assembly.FullName)
            )
        );

        services.AddMediatorHandlers<ModuleMarker>();
        services.AddScoped<ISkillLookUpService, SkillLookUpService>();
    }

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        throw new NotImplementedException();
    }
}