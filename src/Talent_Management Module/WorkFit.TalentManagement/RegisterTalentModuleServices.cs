using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.SharedKernel.DependencyInjection;
using WorkFit.SharedKernel.RegisterModuleServices;
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
    }
}