using Microsoft.EntityFrameworkCore;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployeeById;

public sealed class GetEmployeeByIdHandler
    : IRequestHandler<GetEmployeeByIdQuery, EmployeeDetailDto>
{
    private readonly TalentDbContext _context;

    public GetEmployeeByIdHandler(TalentDbContext context) => _context = context;

    public async Task<EmployeeDetailDto> Handle(GetEmployeeByIdQuery query, CancellationToken ct)
    {
        var employee = await _context.EmployeeProfiles
            .FirstOrDefaultAsync(e => e.Id == query.EmployeeId, ct);

        if (employee is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "Employee", query.EmployeeId);

        return new EmployeeDetailDto(
            employee.Id, employee.Name, employee.Email,
            employee.JobTitle,  employee.IsActive(),
            employee.CurrentAllocationPercentage, employee.HireDate,
            employee.Bio);
    }
}