// GetEmployeeByIdCommandHandler.cs
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Contracts.OrganizationMembership;
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
    private readonly IOrganizationMembershipResolver _orgResolver;

    public GetEmployeeByIdCommandHandler(
        TalentDbContext db, ICurrentUserContext currentUser, IOrganizationMembershipResolver orgResolver)
    {
        _db = db;
        _currentUser = currentUser;
        _orgResolver = orgResolver;
    }

    public async Task<EmployeeDetailsDto> Handle(GetEmployeeByIdCommand request, CancellationToken cancellationToken = default)
    {
        var callerUserId = _currentUser.GetUserId(cancellationToken);
        var organizationId = await _orgResolver.GetOrganizationIdForUserAsync(callerUserId, cancellationToken);

        var employee = await _db.EmployeeProfiles
            .Include(e => e.EmployeeSkills)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && !e.IsDeleted, cancellationToken);

        if (employee is null)
            throw new EntityNotFoundException("TalentManagement", nameof(EmployeeProfile), request.EmployeeId);

        if (employee.OrganizationId != organizationId)
            throw new ForbiddenAccessException("TalentManagement", nameof(EmployeeProfile),
                "This employee belongs to a different organization.");

        if (!request.IsPrivilegedCaller && employee.UserId != callerUserId)
            throw new ForbiddenAccessException("TalentManagement", nameof(EmployeeProfile));

        return new EmployeeDetailsDto(
            employee.Id, employee.OrganizationId, employee.UserId, employee.Name, employee.Email,
            employee.JobTitle, employee.Bio, employee.LinkedInUrl, employee.Status.ToString(),
            employee.CurrentAllocationPercentage,
            employee.EmployeeSkills
                .Select(s => new EmployeeSkillSummaryDto(s.Id, s.SkillId, s.SkillName, s.ConfidenceScore))
                .ToList());
    }
}