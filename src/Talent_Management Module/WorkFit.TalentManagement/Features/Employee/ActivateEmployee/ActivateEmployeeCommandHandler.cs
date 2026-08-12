using Microsoft.EntityFrameworkCore;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.CrossCutting;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Features.Employee.ActivateEmployee;

public sealed class ActivateEmployeeCommandHandler : IRequestHandler<ActivateEmployeeCommand>
{
    private readonly TalentDbContext _context;
    private readonly EmployeeIndexingStatePublisher _publisher;

    public ActivateEmployeeCommandHandler(TalentDbContext context, EmployeeIndexingStatePublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task Handle(ActivateEmployeeCommand command, CancellationToken ct)
    {
        var employee = await _context.EmployeeProfiles
            .FirstOrDefaultAsync(profile => profile.Id == command.EmployeeId, ct);

        if (employee is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "Employee", command.EmployeeId);

        employee.ActivateEmployee();
        await _context.SaveChangesAsync(ct);
        await _publisher.PublishAsync(employee.Id, "Activated", ct);
    }
}
