using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WorkFit.ProjectManagement.Infrastructure.Data.Repositories;
using WorkFit.ProjectManagement.Infrastructure.Seed;
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

        services.AddScoped<IProjectRepository, ProjectRepository>(); // <-- add this


        // Registers all IRequestHandler<,> / IRequestHandler<> / IIntegrationEventHandler<>
        // implementations found in the assembly that contains ModuleMarker
        // (i.e. this ProjectManagement module), including GetProjectsHandler.
        services.AddMediatorHandlers<ModuleMarker>();
    }
}