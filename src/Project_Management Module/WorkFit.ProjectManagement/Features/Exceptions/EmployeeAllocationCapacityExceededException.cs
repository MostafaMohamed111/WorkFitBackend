using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.ProjectManagement.Features.Exceptions;

internal sealed class EmployeeAllocationCapacityExceededException(Guid employeeId, int proposedTotal)
    : FeatureException(
        ModuleMarker.ModuleName,
        "EMPLOYEE_ALLOCATION_CAPACITY_EXCEEDED",
        $"Employee '{employeeId}' allocation would total {proposedTotal}%, exceeding the 100% capacity.",
        "The employee's active task allocation cannot exceed 100%.");
