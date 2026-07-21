namespace WorkFit.Integration.Infrastructure.Providers.Jira;

public sealed class JiraSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
    public int PageSize { get; set; } = 100;
}
