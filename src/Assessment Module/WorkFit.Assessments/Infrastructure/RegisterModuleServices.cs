using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.Assessments.Contracts.CreateAssessmentService;
using WorkFit.Assessments.CrossCutting;
using WorkFit.Assessments.Infrastructure.Data;
using WorkFit.SharedKernel.DependencyInjection;
using WorkFit.SharedKernel.RegisterModuleServices;

namespace WorkFit.Assessments.Infrastructure;

internal sealed class RegisterModuleServices : IRegisterModuleServices
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException();
        services.AddDbContext<AssessmentDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddMediatorHandlers<ModuleMarker>();
        services.AddScoped<ICreateAssessmentService, CreateAssessmentService>();
    }
}
