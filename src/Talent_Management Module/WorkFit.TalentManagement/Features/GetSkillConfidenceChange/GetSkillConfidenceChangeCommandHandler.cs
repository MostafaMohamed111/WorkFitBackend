using Microsoft.EntityFrameworkCore;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Domain.Entities;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Features.GetSkillConfidenceChange;

public sealed class GetSkillConfidenceChangeCommandHandler
    : IRequestHandler<GetSkillConfidenceChangeCommand, SkillConfidenceChangeDto>
{
    private readonly TalentDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private static readonly string[] PrivilegedRoles = { "TeamLeader", "OrganizationOwner", "SuperAdmin" };


    public GetSkillConfidenceChangeCommandHandler(TalentDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<SkillConfidenceChangeDto> Handle(GetSkillConfidenceChangeCommand request, CancellationToken cancellationToken = default)
    {
        var organizationId = _currentUser.GetOrganizationId(cancellationToken);
        var callerUserId = _currentUser.GetUserId(cancellationToken);
        var callerRoles = _currentUser.GetRoles(cancellationToken);
        var isPrivileged = callerRoles.Any(r => PrivilegedRoles.Contains(r));

        var change = await _db.SkillConfidenceChanges
            .Include(scc => scc.ConfidenceEvidences)
            .FirstOrDefaultAsync(scc => scc.Id == request.Id && !scc.IsDeleted, cancellationToken);

        if (change is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, nameof(SkillConfidenceChange), request.Id);

        var owner = await _db.EmployeeSkills
            .Where(es => es.Id == change.EmployeeSkillId)
            .Select(es => new { es.EmployeeProfileId })
            .FirstAsync(cancellationToken);

        var profile = await _db.EmployeeProfiles
            .Where(ep => ep.Id == owner.EmployeeProfileId)
            .Select(ep => new { ep.OrganizationId, ep.UserId })
            .FirstAsync(cancellationToken);

        if (profile.OrganizationId != organizationId)
            throw new ForbiddenAccessException(ModuleMarker.ModuleName, nameof(SkillConfidenceChange),
                "This record belongs to a different organization.");

        if (!isPrivileged && profile.UserId != callerUserId)
            throw new ForbiddenAccessException(ModuleMarker.ModuleName, nameof(SkillConfidenceChange));

        return new SkillConfidenceChangeDto(
            change.Id,
            change.EmployeeSkillId,
            change.AssessmentId,
            change.OldScore,
            change.NewScore,
            change.CreatedAt,
            change.ConfidenceEvidences
                .Where(e => !e.IsDeleted)
                .OrderBy(e => e.CreatedAt)
                .Select(e => new ConfidenceEvidenceDto(e.Id, e.Source, e.Details, e.CreatedAt))
                .ToList());
    }
}