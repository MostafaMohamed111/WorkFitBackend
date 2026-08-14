using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.CrossCutting;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Features.Employee.UpdateEmployee;

public sealed class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand>
{
    private readonly TalentDbContext _context;
    private readonly EmployeeIndexingStatePublisher _publisher;

    public UpdateEmployeeCommandHandler(TalentDbContext context, EmployeeIndexingStatePublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task Handle(UpdateEmployeeCommand command, CancellationToken ct)
    {
        var employee = await _context.EmployeeProfiles.FindAsync(new object[] { command.EmployeeId }, ct);

        if (employee is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "Employee", command.EmployeeId);

        employee.UpdateEmployeePersonalData(command.FirstName, command.LastName, command.JobTitle);

        await _context.SaveChangesAsync(ct);
        await _publisher.PublishAsync(employee.Id, "Updated", ct);
    }
}
