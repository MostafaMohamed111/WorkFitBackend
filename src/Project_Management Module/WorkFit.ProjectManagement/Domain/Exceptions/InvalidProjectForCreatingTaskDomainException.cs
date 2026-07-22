
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.ProjectManagement.Domain.Exceptions;

internal sealed class InvalidProjectForCreatingTaskDomainException(Guid projectId) : DomainException(
        ModuleMarker.ModuleName,
        "INVALID_PROJECT_FOR_CREATING_TASK",
        $"Project with ID '{projectId}' is not valid for creating a task."
    );
