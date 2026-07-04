using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Features.Employee.DeactivateEmployee;

public sealed class DeactivateEmployeeCommandHandler : IRequestHandler<DeactivateEmployeeCommand>
{
    private readonly TalentDbContext _context;

    public DeactivateEmployeeCommandHandler(TalentDbContext context) => _context = context;

    public async Task Handle(DeactivateEmployeeCommand command, CancellationToken ct)
    {
        var employee = await _context.Employees.FindAsync(new object[] { command.EmployeeId }, ct);

        if (employee is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "Employee", command.EmployeeId);

        if (!employee.IsActive)
            throw new FeatureException(ModuleMarker.ModuleName, "EMPLOYEE_ALREADY_INACTIVE", "Employee is already inactive.");

        employee.Deactivate();

        await _context.SaveChangesAsync(ct);
    }
}