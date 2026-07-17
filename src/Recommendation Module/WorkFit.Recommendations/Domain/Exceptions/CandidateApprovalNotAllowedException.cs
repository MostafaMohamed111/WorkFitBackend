using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Recommendations.Domain.Exceptions;

public sealed class CandidateApprovalNotAllowedException : FeatureException
{
    public CandidateApprovalNotAllowedException(Guid employeeId)
        : base(
            ModuleMarker.ModuleName,
            "CANDIDATE_APPROVAL_NOT_ALLOWED",
            $"Employee '{employeeId}' cannot be rejected after being approved.",
            "An approved candidate cannot be rejected.")
    {
    }
}