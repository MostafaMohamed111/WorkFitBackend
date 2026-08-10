using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.Organizations.Domain.Entities;

public sealed class GitHubAppInstallation : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public long GitHubInstallationId { get; private set; }
    public long GitHubOrganizationId { get; private set; }
    public DateTimeOffset InstalledAt { get; private set; }

    public Organization Organization { get; private set; } = default!;

    private GitHubAppInstallation() : base()
    {
    }

    public static GitHubAppInstallation Create(
        Guid organizationId,
        long githubInstallationId,
        long githubOrganizationId,
        DateTimeOffset installedAt)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id is required.", nameof(organizationId));
        }

        if (githubInstallationId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(githubInstallationId));
        }

        if (githubOrganizationId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(githubOrganizationId));
        }

        return new GitHubAppInstallation
        {
            OrganizationId = organizationId,
            GitHubInstallationId = githubInstallationId,
            GitHubOrganizationId = githubOrganizationId,
            InstalledAt = installedAt
        };
    }

    public void Update(long githubInstallationId, long githubOrganizationId, DateTimeOffset installedAt)
    {
        if (githubInstallationId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(githubInstallationId));
        }

        if (githubOrganizationId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(githubOrganizationId));
        }

        GitHubInstallationId = githubInstallationId;
        GitHubOrganizationId = githubOrganizationId;
        InstalledAt = installedAt;
        MarkUpdated();
    }
}
