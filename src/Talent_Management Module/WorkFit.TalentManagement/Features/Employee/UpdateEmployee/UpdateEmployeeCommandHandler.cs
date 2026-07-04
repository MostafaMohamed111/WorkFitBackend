using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Features.Employee.UpdateEmployee;

public sealed class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand>
{
    private readonly TalentDbContext _context;

    public UpdateEmployeeCommandHandler(TalentDbContext context) => _context = context;

    public async Task Handle(UpdateEmployeeCommand command, CancellationToken ct)
    {
        var employee = await _context.Employees.FindAsync(new object[] { command.EmployeeId }, ct);

        if (employee is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "Employee", command.EmployeeId);

        employee.UpdateDetails(command.FirstName, command.LastName, command.JobTitle);

        await _context.SaveChangesAsync(ct);
    }
}