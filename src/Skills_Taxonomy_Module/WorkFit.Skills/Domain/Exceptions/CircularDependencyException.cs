using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Skills.Domain.Exceptions;

public sealed class CircularDependencyException : DomainException
{
    public CircularDependencyException(Guid skillId)
        : base(
            ModuleMarker.ModuleName,
            "CIRCULAR_DEPENDENCY",
            $"Circular dependency detected for skill '{skillId}'.",
            "A circular dependency was detected in skill prerequisites."
        )
    {
    }
}