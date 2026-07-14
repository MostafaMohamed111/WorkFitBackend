using System;
using System.Collections.Generic;
using System.Text;
using WorkFit.Assessments.Domain.Enums;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Assessments.Domain.Exceptions;

internal class InvalidAssessmentStatusTransitionDomainException(AssessmentStatus currentStatus, AssessmentStatus attemptedStatus) : DomainException(
        ModuleMarker.ModuleName,
        "INVALID_ASSESSMENT_STATUS_TRANSITION",
        $"Invalid transition from '{currentStatus.ToString()}' to '{attemptedStatus.ToString()}'."
    )
{
}
