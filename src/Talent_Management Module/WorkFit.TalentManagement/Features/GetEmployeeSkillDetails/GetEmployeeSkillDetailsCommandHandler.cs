using Microsoft.EntityFrameworkCore;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Domain.Entities;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Features.GetEmployeeSkillDetails;

public sealed class GetEmployeeSkillDetailsCommandHandler
    : IRequestHandler<GetEmployeeSkillDetailsCommand, EmployeeSkillDetailsDto>
{
    private static readonly string[] PrivilegedRoles = { "TeamLeader", "OrganizationOwner", "SuperAdmin" };

    private readonly TalentDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetEmployeeSkillDetailsCommandHandler(TalentDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EmployeeSkillDetailsDto> Handle(GetEmployeeSkillDetailsCommand request, CancellationToken cancellationToken = default)
    {
        var organizationId = _currentUser.GetOrganizationId(cancellationToken);
        var callerUserId = _currentUser.GetUserId(cancellationToken);
        var callerRoles = _currentUser.GetRoles(cancellationToken);
        var isPrivileged = callerRoles.Any(r => PrivilegedRoles.Contains(r));

        var skill = await _db.EmployeeSkills
            .Include(s => s.ConfidenceChanges)
            .FirstOrDefaultAsync(s => s.Id == request.EmployeeSkillId && !s.IsDeleted, cancellationToken);

        // Genuinely doesn't exist — the only case that stays 404.
        if (skill is null)
            throw new EntityNotFoundException("TalentManagement", nameof(EmployeeSkill), request.EmployeeSkillId);

        var owner = await _db.EmployeeProfiles
            .Where(ep => ep.Id == skill.EmployeeProfileId)
            .Select(ep => new { ep.OrganizationId, ep.UserId })
            .FirstAsync(cancellationToken);

        // Exists, but caller is in a different org entirely — now Forbidden, not NotFound.
        if (owner.OrganizationId != organizationId)
            throw new ForbiddenAccessException("TalentManagement", nameof(EmployeeSkill),
                "This skill belongs to a different organization.");

        // Same org, but a non-privileged caller viewing someone else's skill — also Forbidden.
        if (!isPrivileged && owner.UserId != callerUserId)
            throw new ForbiddenAccessException("TalentManagement", nameof(EmployeeSkill));

        return new EmployeeSkillDetailsDto(
            skill.Id,
            skill.EmployeeProfileId,
            skill.SkillId,
            skill.SkillName,
            skill.ConfidenceScore,
            skill.ConfidenceChanges
                .OrderBy(c => c.CreatedAt)
                .Select(c => new ConfidenceChangeSummaryDto(c.Id, c.AssessmentId, c.OldScore, c.NewScore, c.CreatedAt))
                .ToList());
    }
}