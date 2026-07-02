using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Skills.Domain.Exceptions;

public sealed class GroupInUseException : FeatureException
{
    public GroupInUseException(Guid groupId)
        : base(
            ModuleMarker.ModuleName,
            "GROUP_IN_USE",
            $"Group with ID '{groupId}' has associated skills and cannot be deleted.",
            "This group has skills assigned and cannot be deleted."
        )
    {
    }
}