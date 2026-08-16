using System.Security.Cryptography;
using System.Text;

namespace WorkFit.WorkFlow.Invitations;

public sealed class DeveloperInvitation
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrganizationId { get; private set; }
    public Guid ProjectId { get; private set; }
    public Guid EmployeeProfileId { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public string Email { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public string SourceSystem { get; private set; } = default!;
    public string SourceAccountId { get; private set; } = default!;
    public string Status { get; private set; } = "Pending";
    public DateTimeOffset RequestedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid? ReviewedByUserId { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }
    public string? TokenHash { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public DateTimeOffset? AcceptedAt { get; private set; }
    public string DeliveryState { get; private set; } = "NotAttempted";
    public string? DeliveryError { get; private set; }
    public Guid? ProvisionedUserId { get; private set; }

    private DeveloperInvitation() { }

    public static DeveloperInvitation Create(Guid organizationId, Guid projectId, Guid employeeProfileId, Guid requesterId, string email, string displayName, string sourceAccountId) =>
        new()
        {
            OrganizationId = organizationId,
            ProjectId = projectId,
            EmployeeProfileId = employeeProfileId,
            RequestedByUserId = requesterId,
            Email = (email ?? string.Empty).Trim(),
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Developer" : displayName.Trim(),
            SourceSystem = "Jira",
            SourceAccountId = (sourceAccountId ?? string.Empty).Trim()
        };

    public void Reject(Guid reviewerId)
    {
        if (Status != "Pending") throw new InvalidOperationException("Only pending invitations can be rejected.");
        Status = "Rejected";
        ReviewedByUserId = reviewerId;
        ReviewedAt = DateTimeOffset.UtcNow;
    }

    public string Approve(Guid reviewerId, TimeSpan lifetime)
    {
        if (Status is not ("Pending" or "Approved")) throw new InvalidOperationException("Only pending or undelivered approved invitations can be approved.");
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        TokenHash = ComputeTokenHash(token);
        ExpiresAt = DateTimeOffset.UtcNow.Add(lifetime);
        Status = "Approved";
        ReviewedByUserId = reviewerId;
        ReviewedAt = DateTimeOffset.UtcNow;
        return token;
    }

    public void SetDelivery(string state, string? error = null) { DeliveryState = state; DeliveryError = error; }
    public void SetProvisionedUser(Guid userId) => ProvisionedUserId ??= userId;
    public void Accept() { Status = "Accepted"; AcceptedAt = DateTimeOffset.UtcNow; }
    public static string ComputeTokenHash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
