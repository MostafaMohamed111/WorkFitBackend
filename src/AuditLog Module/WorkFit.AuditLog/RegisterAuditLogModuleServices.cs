using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.AuditLog.Contracts.Services;
using WorkFit.AuditLog.CrossModule;
using WorkFit.AuditLog.Infrastructure.Data;
using WorkFit.SharedKernel.RegisterModuleServices;

namespace WorkFit.AuditLog;

public sealed class RegisterAuditLogModuleServices : IRegisterModuleServices
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AuditLogDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAuditLogService, AuditLogService>();
    }
}
