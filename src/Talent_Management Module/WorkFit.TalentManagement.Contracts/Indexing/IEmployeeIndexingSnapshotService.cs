namespace WorkFit.TalentManagement.Contracts.Indexing;

public interface IEmployeeIndexingSnapshotService
{
    Task<EmployeeIndexingSnapshot?> GetEmployeeAsync(
        Guid employeeProfileId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmployeeIndexingSnapshot>> GetOrganizationEmployeesAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
