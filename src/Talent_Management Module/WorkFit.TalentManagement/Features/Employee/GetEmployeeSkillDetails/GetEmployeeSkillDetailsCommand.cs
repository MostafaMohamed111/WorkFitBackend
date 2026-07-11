using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployeeSkillDetails;

public sealed record GetEmployeeSkillDetailsCommand(Guid EmployeeSkillId) : IRequest<EmployeeSkillDetailsDto>;

public sealed record EmployeeSkillDetailsDto(
    Guid EmployeeSkillId,
    Guid EmployeeProfileId,
    Guid SkillId,
    string SkillName,
    int ConfidenceScore,
    IReadOnlyList<ConfidenceChangeSummaryDto> ConfidenceChanges);

public sealed record ConfidenceChangeSummaryDto(
    Guid Id,
    Guid AssessmentId,
    int OldScore,
    int NewScore,
    DateTime CreatedAt);


///*//login data
//  "email": "ashrawda@gmail.com",
//  "password": "Rawda123@2",
//  "roles": [
//    "TeamLeader",
//    "SuperAdmin"
// //*/
//{
//  "email": "ashrawda@gmail.com",
//  "password": "Rawda123@2",
//  "twoFactorCode": "123456",
//  "twoFactorRecoveryCode": "123456"
//}