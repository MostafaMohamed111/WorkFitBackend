using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.Integration.Domain.Entities;

/// <summary>
/// Stores integration credentials and configuration for a specific organization
/// and provider (e.g. Jira, GitHub). Each organization may have at most one
/// setting record per provider.
/// </summary>
public sealed class OrganizationIntegrationSetting : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string Provider { get; private set; } = null!;
    public string BaseUrl { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string ApiToken { get; private set; } = null!;
    public string ProjectKey { get; private set; } = null!;
    public int PageSize { get; private set; } = 100;

    private OrganizationIntegrationSetting() : base() { } // EF

    private OrganizationIntegrationSetting(
        Guid organizationId,
        string provider,
        string baseUrl,
        string email,
        string apiToken,
        string projectKey,
        int pageSize) : base()
    {
        OrganizationId = organizationId;
        Provider = provider;
        BaseUrl = baseUrl;
        Email = email;
        ApiToken = apiToken;
        ProjectKey = projectKey;
        PageSize = pageSize;
    }

    public static OrganizationIntegrationSetting Create(
        Guid organizationId,
        string provider,
        string baseUrl,
        string email,
        string apiToken,
        string projectKey,
        int pageSize = 100)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("OrganizationId must not be empty.", nameof(organizationId));
        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentException("Provider must not be empty.", nameof(provider));
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("BaseUrl must not be empty.", nameof(baseUrl));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email must not be empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(apiToken))
            throw new ArgumentException("ApiToken must not be empty.", nameof(apiToken));
        if (string.IsNullOrWhiteSpace(projectKey))
            throw new ArgumentException("ProjectKey must not be empty.", nameof(projectKey));

        return new OrganizationIntegrationSetting(
            organizationId, provider, baseUrl, email, apiToken, projectKey, pageSize);
    }

    public void Update(
        string baseUrl,
        string email,
        string apiToken,
        string projectKey,
        int pageSize)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("BaseUrl must not be empty.", nameof(baseUrl));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email must not be empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(apiToken))
            throw new ArgumentException("ApiToken must not be empty.", nameof(apiToken));
        if (string.IsNullOrWhiteSpace(projectKey))
            throw new ArgumentException("ProjectKey must not be empty.", nameof(projectKey));

        BaseUrl = baseUrl;
        Email = email;
        ApiToken = apiToken;
        ProjectKey = projectKey;
        PageSize = pageSize;
        MarkUpdated();
    }
}
