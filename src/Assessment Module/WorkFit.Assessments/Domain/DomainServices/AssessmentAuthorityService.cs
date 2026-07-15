

using WorkFit.Assessments.Domain.Entities;
using WorkFit.Assessments.Domain.Enums;

namespace WorkFit.Assessments.Domain.DomainServices
{
    internal sealed class AssessmentAuthorityService
    {
        public void Validate(Assessment assessment, Guid currentUserId, Guid? teamLeadId = null)
        {
            if (assessment.Type == AssessmentType.EmployeeProfileSelfAssessment)
            {
                if (teamLeadId.HasValue)
                    throw new ArgumentException();
                if (currentUserId != assessment.EmployeeProfileId)
                    throw new ArgumentException();
            }
            else if (assessment.Type == AssessmentType.TeamLeadAssessment)
            {
                if(teamLeadId is null)
                    throw new ArgumentException();
                if (currentUserId != teamLeadId.Value)
                    throw new ArgumentException();
            }
        }
    }
}
