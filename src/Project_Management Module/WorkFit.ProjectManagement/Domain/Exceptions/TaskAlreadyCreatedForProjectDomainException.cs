

using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.ProjectManagement.Domain.Exceptions;

internal sealed class TaskAlreadyCreatedForProjectDomainException(string title) : DomainException(
        ModuleMarker.ModuleName,
        "TASK_ALREADY_CREATED_FOR_PROJECT",
        $"Task with title '{title}' has already been created for the project."
    );
