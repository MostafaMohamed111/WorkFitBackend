using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.ProjectManagement.Contracts.LookUpServices.TaskLookUp;
using WorkFit.ProjectManagement.CrossCutting;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.ProjectManagement.Infrastructure.Data.Repositories;
using WorkFit.ProjectManagement.Contracts.CreateProjectService;
using WorkFit.ProjectManagement.Contracts.CreateProjectTaskService;
using WorkFit.SharedKernel.DependencyInjection;
using WorkFit.SharedKernel.RegisterModuleServices;

namespace WorkFit.ProjectManagement;

internal class RegisterProjectManagementServices : IRegisterModuleServices
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<WorkFitProjectDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddMediatorHandlers<ModuleMarker>();
        services.AddScoped<ITaskLookUpService, TaskLookUpService>();
        services.AddScoped<ICreateProjectService, CreateProjectService>();
        services.AddScoped<ICreateProjectTaskService, CreateProjectTaskService>();
    }
}
