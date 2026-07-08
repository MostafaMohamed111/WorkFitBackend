using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.SharedKernel.DependencyInjection;
using WorkFit.SharedKernel.RegisterModuleServices;
using WorkFit.Skills.Contracts;
using WorkFit.Skills.Features;
using WorkFit.Skills.Infrastructure.Data;
using WorkFit.Skills.Infrastructure.Similarity;

namespace WorkFit.Skills;

public sealed class RegisterSkillsModuleServices : IRegisterModuleServices
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<WorkFitSkillsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddMediatorHandlers<ModuleMarker>();

        services.AddScoped<ISkillCatalog, SkillCatalogService>();
        services.AddScoped<ISkillSimilarityService, NoOpSkillSimilarityService>();
    }
}