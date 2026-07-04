using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Features.Employee.OnboardEmployee;

public sealed class OnboardEmployeeCommandHandler
    : IRequestHandler<OnboardEmployeeCommand, OnboardEmployeeResponse>
{
    private readonly TalentDbContext _context;

    public OnboardEmployeeCommandHandler(TalentDbContext context) => _context = context;

    public async Task<OnboardEmployeeResponse> Handle(OnboardEmployeeCommand command, CancellationToken ct)
    {
        var employee = Domain.Entities.Employee.Create(
            command.OrganizationId, command.DepartmentId, command.UserId,
            command.FirstName, command.LastName, command.Email,
            command.JobTitle, command.HireDate);

        _context.Employees.Add(employee);

        var profile = Domain.Entities.TalentProfile.Create(employee.Id);
        _context.TalentProfiles.Add(profile);

        await _context.SaveChangesAsync(ct);

        return new OnboardEmployeeResponse(employee.Id);
    }
}