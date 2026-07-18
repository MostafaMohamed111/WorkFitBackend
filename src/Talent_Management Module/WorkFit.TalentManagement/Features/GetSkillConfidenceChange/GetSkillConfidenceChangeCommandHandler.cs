using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Contracts.OrganizationMembership;
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
    private readonly IOrganizationMembershipResolver _orgResolver;

    public GetSkillConfidenceChangeCommandHandler(
        TalentDbContext db, ICurrentUserContext currentUser, IOrganizationMembershipResolver orgResolver)
    {
        _db = db;
        _currentUser = currentUser;
        _orgResolver = orgResolver;
    }

    public async Task<SkillConfidenceChangeDto> Handle(GetSkillConfidenceChangeCommand request, CancellationToken cancellationToken = default)
    {
        var callerUserId = _currentUser.GetUserId(cancellationToken);
        var organizationId = await _orgResolver.GetOrganizationIdForUserAsync(callerUserId, cancellationToken);

        var change = await _db.SkillConfidenceChanges
            .Include(scc => scc.ConfidenceEvidences)
            .FirstOrDefaultAsync(scc => scc.Id == request.Id && !scc.IsDeleted, cancellationToken);

        if (change is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, nameof(SkillConfidenceChange), request.Id);

 
        var employeeSkill = await _db.EmployeeSkills
            .Where(es => es.Id == change.EmployeeSkillId)
            .Select(es => new { es.EmployeeProfileId })
            .FirstAsync(cancellationToken);

        var owner = await _db.EmployeeProfiles
            .Where(ep => ep.Id == employeeSkill.EmployeeProfileId)
            .Select(ep => new { ep.OrganizationId, ep.UserId })
            .FirstAsync(cancellationToken);

        if (owner.OrganizationId != organizationId)
            throw new ForbiddenAccessException(ModuleMarker.ModuleName, nameof(SkillConfidenceChange),
                "This record belongs to a different organization.");

        if (!request.IsPrivilegedCaller && owner.UserId != callerUserId)
            throw new ForbiddenAccessException(ModuleMarker.ModuleName, nameof(SkillConfidenceChange),
                "You do not have permission to view this record.");

        return new SkillConfidenceChangeDto(
            change.Id, change.EmployeeSkillId, change.AssessmentId, change.OldScore, change.NewScore, change.CreatedAt,
            change.ConfidenceEvidences.Where(e => !e.IsDeleted).OrderBy(e => e.CreatedAt)
                .Select(e => new ConfidenceEvidenceDto(e.Id, e.Source, e.Details, e.CreatedAt))
                .ToList());
    }
}