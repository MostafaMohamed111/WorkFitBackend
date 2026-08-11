using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using WorkFit.CodeReview.Infrastructure.Services;
using WorkFit.CodeReview.Infrastructure.Services.Models;
using WorkFit.Organizations.Features.OrganizationsMe;
using WorkFit.Organizations.Contracts.OrganizationGitHub;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.CrossCutting;

internal sealed class GitHubProjectProvisioningService : IGitHubProjectProvisioningService
{
    private readonly IMediator _mediator;
    private readonly IGitHubOrganizationInstallationLookupService _installationLookup;
    private readonly IGitHubAppAuthenticationService _appAuthenticationService;
    private readonly IGitHubCodeReviewService _gitHubService;
    private readonly ILogger<GitHubProjectProvisioningService> _logger;

    public GitHubProjectProvisioningService(
        IMediator mediator,
        IGitHubOrganizationInstallationLookupService installationLookup,
        IGitHubAppAuthenticationService appAuthenticationService,
        IGitHubCodeReviewService gitHubService,
        ILogger<GitHubProjectProvisioningService> logger)
    {
        _mediator = mediator;
        _installationLookup = installationLookup;
        _appAuthenticationService = appAuthenticationService;
        _gitHubService = gitHubService;
        _logger = logger;
    }

    public async Task<GitHubRepositoryCreationResult> CreateProjectRepositoryAsync(
        Guid organizationId,
        Guid projectId,
        string projectName,
        CancellationToken cancellationToken = default)
    {
        var context = await ResolveGitHubContextAsync(organizationId, cancellationToken);
        var repositoryName = BuildRepositoryName(projectName, projectId);

        var repository = await _gitHubService.CreateRepositoryAsync(
            context.OrganizationLogin,
            repositoryName,
            context.AccessToken,
            $"WorkFit project {projectName}",
            cancellationToken);

        _logger.LogInformation(
            "Provisioned GitHub repository {RepositoryName} ({RepositoryId}) for project {ProjectId}.",
            repository.Name,
            repository.Id,
            projectId);

        return repository;
    }

    public async Task<GitHubBranchCreationResult> CreateTaskBranchAsync(
        Guid organizationId,
        Guid projectId,
        string? repositoryName,
        string projectName,
        string taskName,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var context = await ResolveGitHubContextAsync(organizationId, cancellationToken);
        var effectiveRepositoryName = repositoryName ?? BuildRepositoryName(projectName, projectId);
        var repository = await _gitHubService.GetRepositoryMetadataAsync(
            context.OrganizationLogin,
            effectiveRepositoryName,
            context.AccessToken,
            cancellationToken);

        var branchName = BuildBranchName(projectName, taskName, taskId);
        var branch = await _gitHubService.CreateBranchAsync(
            context.OrganizationLogin,
            effectiveRepositoryName,
            branchName,
            repository.DefaultBranch,
            context.AccessToken,
            cancellationToken);

        _logger.LogInformation(
            "Provisioned GitHub branch {BranchName} for task {TaskId} in repository {RepositoryName}.",
            branchName,
            taskId,
            effectiveRepositoryName);

        return branch;
    }

    private async Task<GitHubContext> ResolveGitHubContextAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        var organization = await _mediator.Send(new GetOrganizationGitHubInfoQuery(organizationId), cancellationToken);
        if (string.IsNullOrWhiteSpace(organization.GitHubOrganizationLogin))
        {
            throw new InvalidOperationException($"Organization '{organizationId}' is not connected to GitHub.");
        }

        var installationId = await _installationLookup.GetGitHubInstallationIdForOrganizationAsync(
            organization.GitHubOrganizationLogin,
            cancellationToken);

        if (!installationId.HasValue)
        {
            throw new InvalidOperationException(
                $"GitHub installation for organization '{organization.GitHubOrganizationLogin}' was not found.");
        }

        var accessToken = await _appAuthenticationService.GetInstallationAccessTokenAsync(installationId.Value, cancellationToken);
        return new GitHubContext(organization.GitHubOrganizationLogin, accessToken);
    }

    private static string BuildRepositoryName(string projectName, Guid projectId)
    {
        var projectSlug = BuildSlug(projectName, 40);
        var projectIdSuffix = projectId.ToString("N")[..8];
        return $"workfit-{projectSlug}-{projectIdSuffix}";
    }

    private static string BuildBranchName(string projectName, string taskName, Guid taskId)
    {
        var projectSlug = BuildSlug(projectName, 50);
        var taskSlug = BuildSlug(taskName, 80);
        var branchName = $"workfit-{projectSlug}-{taskSlug}";

        if (branchName.Length <= 240)
        {
            return branchName;
        }

        // Keep a readable prefix while guaranteeing the value fits GitHub and our DB column.
        var suffix = $"-{taskId:N}";
        var maxLength = Math.Max(32, 240 - suffix.Length);
        branchName = branchName[..Math.Min(branchName.Length, maxLength)] + suffix;
        return branchName.Trim('-');
    }

    private static string BuildSlug(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "untitled";
        }

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(ch);
                continue;
            }

            if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        var slug = Regex.Replace(builder.ToString(), "-{2,}", "-").Trim('-');
        if (slug.Length == 0)
        {
            slug = "untitled";
        }

        return slug.Length <= maxLength ? slug : slug[..maxLength].Trim('-');
    }

    private sealed record GitHubContext(string OrganizationLogin, string AccessToken);
}
