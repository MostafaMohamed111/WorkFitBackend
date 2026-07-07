using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
namespace WorkFit.ProjectManagement.Domain.Exceptions;

public sealed class CannotDeleteCompletedTaskException : FeatureException
{
    public CannotDeleteCompletedTaskException(Guid taskId)
        : base(ModuleMarker.ModuleName,
            "CANNOT_DELETE_COMPLETED_TASK",
            $"Task '{taskId}' is already completed and cannot be deleted.",
            "Completed tasks cannot be deleted.")
    {
    }
}