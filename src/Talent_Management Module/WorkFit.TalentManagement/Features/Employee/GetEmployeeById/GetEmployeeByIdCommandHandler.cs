using Microsoft.EntityFrameworkCore;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Domain.Entities;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployeeById;

public sealed class GetEmployeeByIdCommandHandler
    : IRequestHandler<GetEmployeeByIdCommand, EmployeeDetailsDto>
{
    private readonly TalentDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetEmployeeByIdCommandHandler(TalentDbContext db,
        ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EmployeeDetailsDto> Handle(GetEmployeeByIdCommand request, CancellationToken cancellationToken = default)
    {
        var callerUserId = _currentUser.GetUserId(cancellationToken);
        var roles = _currentUser.GetRoles(cancellationToken);
        var isAuthorizedRole = roles.Any(r => r is "SuperAdmin" or "Admin" or "OrganizationOwner" or "TeamLeader");

        var callerEmployee = await _db.EmployeeProfiles
            .FirstOrDefaultAsync(e => (e.UserId == callerUserId || e.Id == callerUserId) && !e.IsDeleted, cancellationToken);

        var employee = await _db.EmployeeProfiles
            .Include(e => e.EmployeeSkills)
            .FirstOrDefaultAsync(e => (e.Id == request.EmployeeId || e.UserId == request.EmployeeId) && !e.IsDeleted, cancellationToken)
            ?? throw new EntityNotFoundException("TalentManagement", nameof(EmployeeProfile), request.EmployeeId);

        if (!isAuthorizedRole && callerEmployee != null && employee.OrganizationId != callerEmployee.OrganizationId)
        {
            throw new ForbiddenAccessException("TalentManagement", nameof(EmployeeProfile),
                "This employee belongs to a different organization.");
        }

        return new EmployeeDetailsDto(
            employee.Id,
            employee.OrganizationId,
            employee.UserId,
            employee.Name,
            employee.Email,
            employee.JobTitle,
            employee.Bio,
            employee.LinkedInUrl,
            employee.Status.ToString(),
            employee.CurrentAllocationPercentage,
            employee.EmployeeSkills
                .Select(s => new EmployeeSkillSummaryDto(s.Id, s.SkillId, s.SkillName, s.ConfidenceScore))
                .ToList());
    }
}