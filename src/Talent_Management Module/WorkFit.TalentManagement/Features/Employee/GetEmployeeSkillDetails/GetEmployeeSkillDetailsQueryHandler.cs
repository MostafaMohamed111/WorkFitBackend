using Microsoft.EntityFrameworkCore;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Domain.Entities;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployeeSkillDetails;

public sealed class GetEmployeeSkillDetailsQueryHandler
    : IRequestHandler<GetEmployeeSkillDetailsQuery, EmployeeSkillDetailsDto>
{

    private readonly TalentDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetEmployeeSkillDetailsQueryHandler(TalentDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EmployeeSkillDetailsDto> Handle(GetEmployeeSkillDetailsQuery query, CancellationToken cancellationToken = default)
    {
        var callerUserId = _currentUser.GetUserId(cancellationToken);


        var callerEmployee = await _db.EmployeeProfiles.AsNoTracking()
            .Where(ep => ep.UserId == callerUserId && !ep.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new EntityNotFoundException("TalentManagement", nameof(EmployeeProfile), callerUserId);

        var employee = await _db.EmployeeProfiles.AsNoTracking()
            .Include(e => e.EmployeeSkills)
            .Where(ep => ep.Id == query.EmployeeId && !ep.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new EntityNotFoundException("TalentManagement", nameof(EmployeeProfile), query.EmployeeSkillId);

        var skill = employee.EmployeeSkills.FirstOrDefault(s => s.Id == query.EmployeeSkillId)
            ?? throw new EntityNotFoundException("TalentManagement", nameof(EmployeeSkill), query.EmployeeSkillId);

        // Exists, but caller is in a different org entirely — now Forbidden, not NotFound.
        if (employee.OrganizationId != callerEmployee.OrganizationId)
            throw new ForbiddenAccessException("TalentManagement", nameof(EmployeeSkill),
                "This skill belongs to a different organization.");

      
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