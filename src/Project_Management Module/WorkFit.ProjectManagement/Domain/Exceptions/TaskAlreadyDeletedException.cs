using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
namespace WorkFit.ProjectManagement.Domain.Exceptions;

public sealed class TaskAlreadyDeletedException : FeatureException
{
    public TaskAlreadyDeletedException(Guid taskId)
        : base(ModuleMarker.ModuleName, "TASK_ALREADY_DELETED",
            $"Task '{taskId}' is already deleted.",
            "This task has already been deleted.")
    {
    }
}
