using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Recommendations.Domain.Exceptions;

public sealed class CandidateAlreadyRejectedException : FeatureException
{
    public CandidateAlreadyRejectedException(Guid employeeId)
        : base(
            ModuleMarker.ModuleName,
            "CANDIDATE_ALREADY_REJECTED",
            $"Employee '{employeeId}' has already been rejected.",
            "The selected candidate has already been rejected.")
    {
    }
}