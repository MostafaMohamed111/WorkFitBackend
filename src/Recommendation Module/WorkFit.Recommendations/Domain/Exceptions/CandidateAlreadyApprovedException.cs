using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Recommendations.Domain.Exceptions;

public sealed class CandidateAlreadyApprovedException : FeatureException
{
    public CandidateAlreadyApprovedException(Guid employeeId)
        : base(
            ModuleMarker.ModuleName,
            "CANDIDATE_ALREADY_APPROVED",
            $"Employee '{employeeId}' has already been approved.",
            "The selected candidate has already been approved.")
    {
    }
}