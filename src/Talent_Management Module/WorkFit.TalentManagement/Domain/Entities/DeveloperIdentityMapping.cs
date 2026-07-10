using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.TalentManagement.Domain.Entities;

internal sealed class DeveloperIdentityMapping : BaseEntity
{
    public Guid EmployeeProfileId { get; private set; }
    public string SourceSystem { get; private set; } = default!;
    public string ExternalAccountId { get; private set; } = default!;
    public string ExternalDisplayName { get; private set; } = default!;

    public EmployeeProfile Employee { get; private set; } = default!;

    // For EF Core
    private DeveloperIdentityMapping() { }

    internal static DeveloperIdentityMapping Create(
        Guid employeeProfileId,
        string sourceSystem,
        string externalAccountId,
        string externalDisplayName)
    {
        return new DeveloperIdentityMapping
        {
            EmployeeProfileId = employeeProfileId,
            SourceSystem = sourceSystem,
            ExternalAccountId = externalAccountId,
            ExternalDisplayName = externalDisplayName
        };
    }
}
