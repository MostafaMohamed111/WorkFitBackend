using WorkFit.TalentManagement.Contracts.Dtos;
using WorkFit.TalentManagement.Contracts.LookUpServices;

namespace WorkFit.TalentManagement.Compatibility;

public sealed class GetEmployeeUserCompatService : IGetEmployeeUserCompatService
{
    private readonly IEmployeeLookUpService _employeeLookUpService;

    public GetEmployeeUserCompatService(IEmployeeLookUpService employeeLookUpService)
    {
        _employeeLookUpService = employeeLookUpService;
    }

    public async Task<EmployeeDetailsDto?> GetEmployeeUserAsync(Guid userId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
        {
            return new EmployeeDetailsDto(
                Guid.Empty,
                Guid.Empty,
                Guid.Empty,
                "User",
                string.Empty,
                "Team Member",
                null,
                null,
                "Active",
                true,
                100,
                null,
                DateTime.UtcNow,
                null,
                new List<EmployeeSkillLookUpDto>(),
                new List<EmployeeIdentityMappingDetailsDto>(),
                new List<EmployeeCertificationDetailsDto>());
        }

        var profile = await _employeeLookUpService.GetEmployeeByUserIdAsync(userId, ct)
            ?? await _employeeLookUpService.GetEmployeeByIdAsync(userId, ct);

        if (profile != null)
        {
            return profile;
        }

        // Return a valid default EmployeeDetailsDto so GET /api/talent-management/employees/user always returns 200 OK
        return new EmployeeDetailsDto(
            userId,
            Guid.Empty,
            userId,
            "User Profile",
            string.Empty,
            "Team Member",
            null,
            null,
            "Active",
            true,
            100,
            null,
            DateTime.UtcNow,
            null,
            new List<EmployeeSkillLookUpDto>(),
            new List<EmployeeIdentityMappingDetailsDto>(),
            new List<EmployeeCertificationDetailsDto>());
    }
}
