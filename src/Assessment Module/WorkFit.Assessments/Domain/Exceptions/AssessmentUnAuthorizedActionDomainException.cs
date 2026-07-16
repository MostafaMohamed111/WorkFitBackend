

using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Assessments.Domain.Exceptions;

internal sealed class AssessmentUnAuthorizedActionDomainException(Guid processedById) : DomainException(
        ModuleMarker.ModuleName,
        "ASSESSMENT_UNAUTHORIZED_ACTION",
        $"Not authorized to make action to this assessment. ProcessedById: {processedById}")
{
}
