using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.CrossCutting;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Features.Employee.OnboardEmployee;

public sealed class OnboardEmployeeCommandHandler
    : IRequestHandler<OnboardEmployeeCommand, OnboardEmployeeResponse>
{
    private readonly TalentDbContext _context;
    private readonly EmployeeIndexingStatePublisher _publisher;

    public OnboardEmployeeCommandHandler(TalentDbContext context, EmployeeIndexingStatePublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<OnboardEmployeeResponse> Handle(OnboardEmployeeCommand command, CancellationToken ct)
    {
        var employee = Domain.Entities.EmployeeProfile.Create(
            command.OrganizationId, command.UserId, command.Email,
            command.Name,
            command.JobTitle, command.HireDate);

        _context.EmployeeProfiles.Add(employee);

        await _context.SaveChangesAsync(ct);
        await _publisher.PublishAsync(employee.Id, "Created", ct);

        return new OnboardEmployeeResponse(employee.Id);
    }
}
