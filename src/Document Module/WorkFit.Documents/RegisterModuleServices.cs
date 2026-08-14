using WorkFit.Documents.Contracts;
using WorkFit.Documents.Contracts.AttachDocumentService;
using WorkFit.Documents.Contracts.DocumentContentService;
using WorkFit.Documents.Contracts.DocumentLookUpService;
using WorkFit.Documents.Contracts.TemporaryUploadService;
using WorkFit.Documents.CrossCutting;
using WorkFit.Documents.Infrastructure.Abstractions;
using WorkFit.Documents.Infrastructure.BackgroundWorkers;
using WorkFit.Documents.Infrastructure.Configuration;
using WorkFit.Documents.Infrastructure.Data;
using WorkFit.Documents.Infrastructure.FileStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.SharedKernel.DependencyInjection;
using WorkFit.SharedKernel.RegisterModuleServices;
using Microsoft.Extensions.Hosting;

namespace WorkFit.Documents.Infrastructure;

internal sealed class RegisterModuleServices : IRegisterModuleServices
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ApplicationException("Invalid Connection string");

        services.AddDbContext<DocumentDbContext>(options =>
            options.UseSqlServer(connectionString)
        );

        services.AddScoped<IDocumentLookUpService, DocumentLookUpService>();
        services.AddScoped<ICreateTemporaryDocumentService, CreateTemporaryDocumentService>();
        services.AddScoped<IDocumentContentService, DocumentContentService>();

        services
            .AddOptions<LocalDocumentFileStorageOptions>()
            .Bind(configuration.GetSection(LocalDocumentFileStorageOptions.SectionPath));

        services.AddSingleton<IFileStorage, LocalDocumentFileStorage>();
        services.AddScoped<IAttachTemporaryDocumentService, AttachTemporaryDocumentService>();
        services.AddScoped<IDeleteDocumentService, DeleteDocumentsService>();
        services.AddScoped<ITemporaryUploadOrphanCleanupService, TemporaryUploadOrphanCleanupService>();

        services
            .AddOptions<TemporaryUploadCleanupOptions>()
            .Bind(configuration.GetSection(TemporaryUploadCleanupOptions.SectionName))
            .Validate(
                o =>
                {
                    if (!o.Enabled)
                        return true;
                    return o.MaxBatchSize > 0
                        && o.InitialDelay >= TimeSpan.Zero
                        && o.IntervalWhenIdle > TimeSpan.Zero
                        && o.IntervalWhenWorkDone > TimeSpan.Zero;
                },
                "Documents:TemporaryUploadCleanup: when Enabled is true, MaxBatchSize must be > 0, InitialDelay >= 0, and IntervalWhenIdle / IntervalWhenWorkDone must be > 0. " +
                "Check for bad environment overrides (e.g. empty Documents__TemporaryUploadCleanup__IntervalWhenIdle).")
            .ValidateOnStart();

        services.AddSingleton<IHostedService, TemporaryUploadOrphanCleanupBackgroundService>();

        services.AddMediatorHandlers<ModuleMarker>();
    }
}
