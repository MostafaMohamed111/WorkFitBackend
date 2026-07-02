using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Skills.Domain.Exceptions;

public sealed class CategoryInUseException : FeatureException
{
    public CategoryInUseException(Guid categoryId)
        : base(
            ModuleMarker.ModuleName,
            "CATEGORY_IN_USE",
            $"Category with ID '{categoryId}' has associated groups and cannot be deleted.",
            "This category has groups assigned and cannot be deleted."
        )
    {
    }
}