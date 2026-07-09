
//using WorkFit.SharedKernel.BaseEntity;

//namespace WorkFit.TalentManagement.Domain.Entities;

//internal sealed class TaskAllocation : BaseEntity
//{
//    public Guid TaskId { get; private set; }
//    public int AllocationPercentage { get; private set; }
//    public DateOnly StartDate { get; private set; }
//    public DateOnly EndDate { get; private set; }
//    public bool IsActive
//        => DateOnly.FromDateTime(DateTime.Now) >= StartDate && DateOnly.FromDateTime(DateTime.Now) <= EndDate;

//    private TaskAllocation() { } // for EF

//    private TaskAllocation(Guid taskId, int allocationPercentage, DateOnly startDate, DateOnly endDate)
//    {
//        TaskId = taskId;
//        AllocationPercentage = allocationPercentage;
//        StartDate = startDate;
//        EndDate = endDate;
//    }

//    public static TaskAllocation Create(Guid taskId, int allocationPercentage, DateOnly startDate, DateOnly endDate)
//    {
//        // Validation is here

//        return new TaskAllocation(taskId, allocationPercentage, startDate, endDate);
//    }
//}
