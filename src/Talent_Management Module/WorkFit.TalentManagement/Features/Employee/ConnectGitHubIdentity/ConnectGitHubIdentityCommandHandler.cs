using Microsoft.EntityFrameworkCore;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Domain.Constants;
using WorkFit.TalentManagement.Domain.Entities;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Features.Employee.ConnectGitHubIdentity;

public sealed class ConnectGitHubIdentityCommandHandler : IRequestHandler<ConnectGitHubIdentityCommand>
{
    private readonly TalentDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public ConnectGitHubIdentityCommandHandler(TalentDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(ConnectGitHubIdentityCommand request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUser.GetUserId(cancellationToken);

        var employee = await _db.EmployeeProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == currentUserId && !e.IsDeleted, cancellationToken)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, nameof(EmployeeProfile), currentUserId);

        var accountId = request.GitHubAccountId.Trim();
        var displayName = string.IsNullOrWhiteSpace(request.GitHubDisplayName)
            ? accountId
            : request.GitHubDisplayName.Trim();

        var existingMapping = await _db.IdentityMappings
            .FirstOrDefaultAsync(
                m => m.SourceSystem == ExternalSourceSystems.GitHub &&
                     m.ExternalAccountId == accountId,
                cancellationToken);

        if (existingMapping is not null)
        {
            if (existingMapping.EmployeeProfileId != employee.Id)
            {
                throw new EntityAlreadyExistsException(
                    ModuleMarker.ModuleName,
                    "EmployeeGitHubIdentity",
                    existingMapping.Id);
            }

            if (!string.Equals(existingMapping.ExternalDisplayName, displayName, StringComparison.Ordinal))
            {
                existingMapping.UpdateDisplayName(displayName);
            }
        }
        else
        {
            _db.IdentityMappings.Add(
                DeveloperIdentityMapping.Create(
                    employee.Id,
                    ExternalSourceSystems.GitHub,
                    accountId,
                    displayName));
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
