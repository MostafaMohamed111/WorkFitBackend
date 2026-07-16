

using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Assessments.Domain.Exceptions;

internal sealed class InvalidEmployeeProfileSelfAssessmentTeamLeadIdMustBeNullException() : DomainException(
        ModuleMarker.ModuleName,
        "INVALID_EMPLOYEE_PROFILE_SELF_ASSESSMENT_TEAM_LEAD_ID_MUST_BE_NULL",
        "Employee profile self assessment should not have a team lead id. Team lead id must be null.")
{

}
