using WorkFit.WorkFlow.Invitations;

namespace WorkFit.Rag.Tests;

public sealed class DeveloperInvitationTests
{
    [Fact]
    public void ApproveCreatesRandomHashedTokenAndNeverPersistsPlaintext()
    {
        var invitation = CreateInvitation();

        var token = invitation.Approve(Guid.NewGuid(), TimeSpan.FromHours(48));

        Assert.Equal("Approved", invitation.Status);
        Assert.NotNull(invitation.TokenHash);
        Assert.NotEqual(token, invitation.TokenHash);
        Assert.Equal(DeveloperInvitation.ComputeTokenHash(token), invitation.TokenHash);
        Assert.True(invitation.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public void AcceptedInvitationRetainsHashForIdempotentReplay()
    {
        var invitation = CreateInvitation();
        var token = invitation.Approve(Guid.NewGuid(), TimeSpan.FromHours(48));
        invitation.SetProvisionedUser(Guid.NewGuid());

        invitation.Accept();

        Assert.Equal("Accepted", invitation.Status);
        Assert.Equal(DeveloperInvitation.ComputeTokenHash(token), invitation.TokenHash);
        Assert.NotNull(invitation.AcceptedAt);
    }

    [Fact]
    public void PendingInvitationCanOnlyBeReviewedOnce()
    {
        var invitation = CreateInvitation();
        invitation.Reject(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => invitation.Approve(Guid.NewGuid(), TimeSpan.FromHours(48)));
    }

    [Fact]
    public void ReapprovingAfterDeliveryFailureInvalidatesThePreviousToken()
    {
        var invitation = CreateInvitation();
        var firstToken = invitation.Approve(Guid.NewGuid(), TimeSpan.FromHours(48));
        invitation.SetDelivery("Failed", "SMTP unavailable");

        var replacementToken = invitation.Approve(Guid.NewGuid(), TimeSpan.FromHours(48));

        Assert.NotEqual(firstToken, replacementToken);
        Assert.Equal(DeveloperInvitation.ComputeTokenHash(replacementToken), invitation.TokenHash);
    }

    private static DeveloperInvitation CreateInvitation() => DeveloperInvitation.Create(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
        "developer@example.test", "Jira Developer", "jira-account-1");
}
