using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Recommendations.Domain.Exceptions;

public sealed class RecommendationAccessDeniedException : FeatureException
{
    public RecommendationAccessDeniedException(Guid recommendationId, Guid userId)
        : base(
            ModuleMarker.ModuleName,
            "RECOMMENDATION_ACCESS_DENIED",
            $"User '{userId}' is not authorized to review recommendation '{recommendationId}'.",
            "You are not authorized to approve or reject candidates for this recommendation.")
    {
    }
}