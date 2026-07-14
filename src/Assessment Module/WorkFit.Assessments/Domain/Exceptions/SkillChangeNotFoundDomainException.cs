
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Assessments.Domain.Exceptions;

internal sealed class SkillChangeNotFoundDomainException(Guid skillChangeId) : DomainException(
    ModuleMarker.ModuleName,
        "SKILL_CHANGE_NOT_FOUND",
        $"Skill change with ID '{skillChangeId}' was not found."
    );

