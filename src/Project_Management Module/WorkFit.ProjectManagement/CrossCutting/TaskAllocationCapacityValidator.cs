using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Features.Exceptions;
using WorkFit.ProjectManagement.Infrastructure;
using TaskStatus = WorkFit.ProjectManagement.Domain.Enums.TaskStatus;

namespace WorkFit.ProjectManagement.CrossCutting;

internal static class TaskAllocationCapacityValidator
{
    public static async Task ValidateAsync(
        WorkFitProjectDbContext context,
        Guid employeeId,
        int proposedAllocationPercentage,
        Guid? excludedTaskId,
        CancellationToken cancellationToken)
    {
        var currentAllocation = await context.ProjectTasks
            .AsNoTracking()
            .Where(task =>
                task.AssignedEmployeeId == employeeId &&
                task.Status != TaskStatus.Done &&
                task.AllocationPercentage > 0 &&
                (!excludedTaskId.HasValue || task.Id != excludedTaskId.Value))
            .SumAsync(task => task.AllocationPercentage, cancellationToken);

        var proposedTotal = currentAllocation + proposedAllocationPercentage;
        if (proposedTotal > 100)
            throw new EmployeeAllocationCapacityExceededException(employeeId, proposedTotal);
    }
}
