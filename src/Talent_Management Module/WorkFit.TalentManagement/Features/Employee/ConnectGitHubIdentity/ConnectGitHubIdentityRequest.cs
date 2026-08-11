namespace WorkFit.TalentManagement.Features.Employee.ConnectGitHubIdentity;

public sealed class ConnectGitHubIdentityRequest
{
    public string GitHubAccountId { get; set; } = default!;
    public string GitHubDisplayName { get; set; } = default!;
}
