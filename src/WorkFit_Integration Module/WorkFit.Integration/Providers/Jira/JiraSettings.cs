namespace WorkFit.Integration.Providers.Jira;

/// <summary>
/// Configuration POCO for the Jira integration.
/// Bind from appsettings.json "Jira" section.
/// </summary>
public sealed class JiraSettings
{
    /// <summary>
    /// Jira instance base URL, e.g. "https://your-workspace.atlassian.net"
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Atlassian account e-mail used for Basic Auth.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Jira API token (generated at https://id.atlassian.com/manage-profile/security/api-tokens).
    /// </summary>
    public string ApiToken { get; set; } = string.Empty;

    /// <summary>
    /// The Jira project key to sync (e.g. "EC").
    /// </summary>
    public string ProjectKey { get; set; } = string.Empty;

    /// <summary>
    /// Maximum results per page when paginating Jira search. Default 100.
    /// </summary>
    public int PageSize { get; set; } = 100;
}
