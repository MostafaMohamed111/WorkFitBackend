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
    private static readonly string[] PrivilegedRoles = { "TeamLeader", "OrganizationOwner", "SuperAdmin" };

    private readonly TalentDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetEmployeeByIdCommandHandler(TalentDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EmployeeDetailsDto> Handle(GetEmployeeByIdCommand request, CancellationToken cancellationToken = default)
    {
        var organizationId = _currentUser.GetOrganizationId(cancellationToken);
        var callerUserId = _currentUser.GetUserId(cancellationToken);
        var callerRoles = _currentUser.GetRoles(cancellationToken);
        var isPrivileged = callerRoles.Any(r => PrivilegedRoles.Contains(r));

        var employee = await _db.EmployeeProfiles
            .Include(e => e.EmployeeSkills)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && !e.IsDeleted, cancellationToken);

        // مش موجودة أصلاً — الحالة الوحيدة اللي فعلاً 404
        if (employee is null)
            throw new EntityNotFoundException("TalentManagement", nameof(EmployeeProfile), request.EmployeeId);

        // موجودة، بس من Organization مختلفة تماماً — Forbidden مش NotFound
        if (employee.OrganizationId != organizationId)
            throw new ForbiddenAccessException("TalentManagement", nameof(EmployeeProfile),
                "This employee belongs to a different organization.");

        // نفس الـ Org، بس مستخدم مش Privileged بيحاول يشوف بيانات موظف تاني
        if (!isPrivileged && employee.UserId != callerUserId)
            throw new ForbiddenAccessException("TalentManagement", nameof(EmployeeProfile));

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