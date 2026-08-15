using Microsoft.EntityFrameworkCore;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Domain.Entities;
using WorkFit.TalentManagement.Features.Employee.GetEmployeeById;
using WorkFit.TalentManagement.Features.Employee.GetEmployeeUserById;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployeeByUserId;

public sealed class GetEmployeeByUserIdCommandHandler
    : IRequestHandler<GetEmployeeByUserIdCommand, EmployeeDetailsDto>
{

    private readonly TalentDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetEmployeeByUserIdCommandHandler(TalentDbContext db,
        ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EmployeeDetailsDto> Handle(GetEmployeeByUserIdCommand request, CancellationToken cancellationToken = default)
    {
        var callerUserId = _currentUser.GetUserId(cancellationToken);

        
        var employee = await _db.EmployeeProfiles
            .Include(e => e.EmployeeSkills)
            .FirstOrDefaultAsync(e => e.UserId == callerUserId && !e.IsDeleted, cancellationToken)
            ?? throw new EntityNotFoundException("TalentManagement", nameof(EmployeeProfile), callerUserId);

        
        
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