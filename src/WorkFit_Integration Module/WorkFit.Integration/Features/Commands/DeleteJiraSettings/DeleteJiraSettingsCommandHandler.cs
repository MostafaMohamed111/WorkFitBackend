using Microsoft.EntityFrameworkCore;
using WorkFit.Integration.Infrastructure.Data;
using WorkFit.Integration.Domain.Entities;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Integration.Features.Commands.DeleteJiraSettings;

internal sealed class DeleteJiraSettingsCommandHandler : IRequestHandler<DeleteJiraSettingsCommand>
{
    private readonly IntegrationDbContext _db;

    public DeleteJiraSettingsCommandHandler(IntegrationDbContext db) => _db = db;

    public async Task Handle(DeleteJiraSettingsCommand request, CancellationToken cancellationToken = default)
    {
        var setting = await _db.OrganizationIntegrationSettings
            .FirstOrDefaultAsync(
                s => s.OrganizationId == request.OrganizationId && s.Provider == "Jira", cancellationToken);

        if (setting is null)
        {
            throw new EntityNotFoundException(ModuleMarker.ModuleName, typeof(OrganizationIntegrationSetting).Name, request.OrganizationId);
        }

        _db.OrganizationIntegrationSettings.Remove(setting);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
