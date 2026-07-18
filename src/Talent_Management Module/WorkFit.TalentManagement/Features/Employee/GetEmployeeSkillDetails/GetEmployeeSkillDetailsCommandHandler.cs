using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Contracts.OrganizationMembership;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Domain.Entities;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployeeSkillDetails;

public sealed class GetEmployeeSkillDetailsCommandHandler
    : IRequestHandler<GetEmployeeSkillDetailsCommand, EmployeeSkillDetailsDto>
{
    private readonly TalentDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly IOrganizationMembershipResolver _orgResolver;

    public GetEmployeeSkillDetailsCommandHandler(
        TalentDbContext db, ICurrentUserContext currentUser, IOrganizationMembershipResolver orgResolver)
    {
        _db = db;
        _currentUser = currentUser;
        _orgResolver = orgResolver;
    }

    public async Task<EmployeeSkillDetailsDto> Handle(GetEmployeeSkillDetailsCommand request, CancellationToken cancellationToken = default)
    {
        var callerUserId = _currentUser.GetUserId(cancellationToken);
        var organizationId = await _orgResolver.GetOrganizationIdForUserAsync(callerUserId, cancellationToken);

        var skill = await _db.EmployeeSkills
            .Include(s => s.ConfidenceChanges)
            .FirstOrDefaultAsync(s => s.Id == request.EmployeeSkillId && !s.IsDeleted, cancellationToken);

        if (skill is null)
            throw new EntityNotFoundException("TalentManagement", nameof(EmployeeSkill), request.EmployeeSkillId);

        var owner = await _db.EmployeeProfiles
            .Where(ep => ep.Id == skill.EmployeeProfileId)
            .Select(ep => new { ep.OrganizationId, ep.UserId })
            .FirstAsync(cancellationToken);

        if (owner.OrganizationId != organizationId)
            throw new ForbiddenAccessException("TalentManagement", nameof(EmployeeSkill),
                "This skill belongs to a different organization.");

        if (!request.IsPrivilegedCaller && owner.UserId != callerUserId) 
            throw new ForbiddenAccessException("TalentManagement", nameof(EmployeeSkill),
                "You do not have permission to view this skill.");

        return new EmployeeSkillDetailsDto(
            skill.Id, skill.EmployeeProfileId, skill.SkillId, skill.SkillName, skill.ConfidenceScore,
            skill.ConfidenceChanges.OrderBy(c => c.CreatedAt)
                .Select(c => new ConfidenceChangeSummaryDto(c.Id, c.AssessmentId, c.OldScore, c.NewScore, c.CreatedAt))
                .ToList());
    }
}