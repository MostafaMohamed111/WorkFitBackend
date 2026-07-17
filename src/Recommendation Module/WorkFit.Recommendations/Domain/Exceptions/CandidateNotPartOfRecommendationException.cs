using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Recommendations.Domain.Exceptions;
public sealed class CandidateNotPartOfRecommendationException : FeatureException
{
    public CandidateNotPartOfRecommendationException(Guid recommendationId, Guid employeeId)
        : base(ModuleMarker.ModuleName, "CANDIDATE_NOT_PART_OF_RECOMMENDATION",
            $"Employee '{employeeId}' is not a candidate in recommendation '{recommendationId}'.",
            "This employee is not part of this recommendation's candidate list.")
    {
    }
}