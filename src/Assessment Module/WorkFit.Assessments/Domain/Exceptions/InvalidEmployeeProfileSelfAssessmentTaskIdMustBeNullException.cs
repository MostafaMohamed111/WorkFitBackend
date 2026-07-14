using System;
using System.Collections.Generic;
using System.Text;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Assessments.Domain.Exceptions;

internal sealed class InvalidEmployeeProfileSelfAssessmentTaskIdMustBeNullException() : DomainException(ModuleMarker.ModuleName,
    "INVALID_EMPLOYEE_PROFILE_SELF_ASSESSMENT_TASK_ID_MUST_BE_NULL",
    "Employee profile self-assessment task ID must be null, there is no tasks "
    )
{
}
