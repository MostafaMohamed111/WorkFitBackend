using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.SharedKernel.DependencyInjection;
using WorkFit.SharedKernel.RegisterModuleServices;
using WorkFit.TalentManagement.Contracts.LookUpServices;
using WorkFit.TalentManagement.Contracts.WriteServices.CreateEmployee;
using WorkFit.TalentManagement.Contracts.WriteServices.CreateOrUpdateSkill;
using WorkFit.TalentManagement.CrossCutting;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement;

public sealed class RegisterTalentModuleServices : IRegisterModuleServices
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<TalentDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddMediatorHandlers<ModuleMarker>();
        services.AddScoped<IEmployeeLookUpService, EmployeeLookUpService>();
        services.AddScoped<ICreateEmployeeService, CreateEmployeeService>();
        services.AddScoped<IGetOrCreateExternalEmployeeService, GetOrCreateExternalEmployeeService>();
        services.AddScoped<ICreateOrUpdateEmployeeSkillsAfterAssessmentService, CreateOrUpdateEmployeeSkillsAfterAssessmentService>();
    }
}