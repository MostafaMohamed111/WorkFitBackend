using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.ConnectGitHubIdentity;

public sealed record ConnectGitHubIdentityCommand(
    string GitHubAccountId,
    string GitHubDisplayName) : IRequest;
