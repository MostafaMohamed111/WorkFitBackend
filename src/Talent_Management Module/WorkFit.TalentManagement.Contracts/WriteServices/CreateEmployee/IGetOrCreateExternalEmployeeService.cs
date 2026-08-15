namespace WorkFit.TalentManagement.Contracts.WriteServices.CreateEmployee;

public interface IGetOrCreateExternalEmployeeService
{
    /// <summary>
    /// Looks up a developer by their external identity mapping (e.g. Jira accountId). 
    /// If found, returns the internal EmployeeProfile ID.
    /// If not found, creates a new EmployeeProfile (with optional email), creates the mapping, and returns the ID.
    /// </summary>
    Task<ExternalEmployeeResolution> GetOrCreateAsync(
        Guid organizationId,
        string sourceSystem,
        string externalAccountId,
        string externalDisplayName,
        string? email,
        string jobTitle,
        string? linkedInUrl = null,
        CancellationToken cancellationToken = default);
}

public sealed record ExternalEmployeeResolution(
    Guid EmployeeProfileId,
    bool IsPending,
    bool IsLinked);
