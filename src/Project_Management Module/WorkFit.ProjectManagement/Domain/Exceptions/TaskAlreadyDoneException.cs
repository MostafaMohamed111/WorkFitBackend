using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.ProjectManagement.Domain.Exceptions;

public sealed class TaskAlreadyDoneException : DomainException
{
    public TaskAlreadyDoneException(string moduleName)
        : base(
            moduleName,
            "TASK_ALREADY_DONE",
            "Task is already completed.",
            "A completed task cannot be reopened."
        )
    {
    }
}