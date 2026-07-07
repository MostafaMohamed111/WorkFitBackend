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
        var employee = Domain.Entities.EmployeeProfile.Create(
            command.OrganizationId, command.UserId, command.Email,
            command.Name,
            command.JobTitle, command.HireDate);

        _context.EmployeeProfiles.Add(employee);

        await _context.SaveChangesAsync(ct);

        return new OnboardEmployeeResponse(employee.Id);
    }
}