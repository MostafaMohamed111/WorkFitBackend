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
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<WorkFitSkillsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddMediatorHandlers<ModuleMarker>();
    }
}