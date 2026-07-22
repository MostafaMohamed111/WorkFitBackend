

using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.ProjectManagement.Domain.Exceptions;

internal sealed class TaskNotFoundForProjectDomainException(Guid TaskId, Guid ProjectId) : DomainException(
        ModuleMarker.ModuleName,
        "TASK_NOT_FOUND_FOR_PROJECT",
        $"Task with ID '{TaskId}' was not found for the project with ID '{ProjectId}'."
    );
