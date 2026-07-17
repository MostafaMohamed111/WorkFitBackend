using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.Recommendations.Domain.Services;
using WorkFit.Recommendations.Infrastructure.Data;
using WorkFit.SharedKernel.DependencyInjection;
using WorkFit.SharedKernel.RegisterModuleServices;

namespace WorkFit.Recommendations;

public sealed class RegisterRecommendationModuleServices : IRegisterModuleServices
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<RecommendationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IRecommendationScoringService, RecommendationScoringService>();

        services.AddMediatorHandlers<ModuleMarker>();
    }
}