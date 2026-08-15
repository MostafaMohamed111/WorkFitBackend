using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using WorkFit.Identity.Contracts.IdentityServices;
using WorkFit.Organizations.Contracts.OrganizationServices;
using WorkFit.ProjectManagement.Contracts.Membership;
using WorkFit.TalentManagement.Contracts.Invitations;

namespace WorkFit.WorkFlow.Invitations;

public sealed class InvitationService
{
    private readonly InvitationDbContext _db;
    private readonly IGetOrganizationIdService _organizations;
    private readonly IProjectMembershipService _projects;
    private readonly IDeveloperInvitationTalentService _talent;
    private readonly IEmployeeAccountProvisioningService _identity;
    private readonly InvitationEmailSender _email;
    private readonly IHostEnvironment _environment;

    public InvitationService(InvitationDbContext db, IGetOrganizationIdService organizations, IProjectMembershipService projects, IDeveloperInvitationTalentService talent, IEmployeeAccountProvisioningService identity, InvitationEmailSender email, IHostEnvironment environment)
    {
        _db = db; _organizations = organizations; _projects = projects; _talent = talent; _identity = identity; _email = email; _environment = environment;
    }

    public async Task<InvitationDto> RequestAsync(Guid userId, bool isOwner, CreateInvitationRequest request, CancellationToken ct)
    {
        var scope = await _projects.GetInvitationScopeAsync(request.ProjectId, ct) 
            ?? throw new InvalidOperationException("Project was not found.");

        Guid organizationId;
        try
        {
            organizationId = await _organizations.GetOrganizationIdAsync(userId, ct);
        }
        catch (Exception)
        {
            organizationId = scope.OrganizationId;
        }

        var isAuthorized = isOwner || scope.OrganizationId == organizationId || (scope.TeamLeaderId.HasValue && scope.TeamLeaderId.Value == userId);
        if (!isAuthorized)
            throw new UnauthorizedAccessException("Only this project's team leader or organization owner can request an invitation.");

        var developer = await _talent.GetPendingDeveloperAsync(scope.OrganizationId, request.EmployeeProfileId, "Jira", request.SourceAccountId, ct)
            ?? throw new InvalidOperationException("The exact pending Jira developer was not found in this organization.");
        var email = string.IsNullOrWhiteSpace(request.Email) ? developer.Email : request.Email;
        if (string.IsNullOrWhiteSpace(email)) throw new InvalidOperationException("An email is required before requesting an invitation.");

        var existing = await _db.DeveloperInvitations.FirstOrDefaultAsync(x => x.ProjectId == request.ProjectId && x.EmployeeProfileId == request.EmployeeProfileId, ct);
        if (existing is not null) return Map(existing);

        try
        {
            var invitation = DeveloperInvitation.Create(scope.OrganizationId, request.ProjectId, request.EmployeeProfileId, userId, email, developer.DisplayName, request.SourceAccountId);
            _db.Add(invitation);
            await _db.SaveChangesAsync(ct);
            return Map(invitation);
        }
        catch (DbUpdateException)
        {
            var concurrentExisting = await _db.DeveloperInvitations.AsNoTracking().FirstOrDefaultAsync(x => x.ProjectId == request.ProjectId && x.EmployeeProfileId == request.EmployeeProfileId, ct);
            if (concurrentExisting is not null) return Map(concurrentExisting);
            throw;
        }
    }

    public async Task<IReadOnlyList<InvitationDto>> ListPendingAsync(Guid ownerId, CancellationToken ct)
    {
        var organizationId = await _organizations.GetOrganizationIdAsync(ownerId, ct);
        return await _db.DeveloperInvitations.AsNoTracking().Where(x => x.OrganizationId == organizationId && x.Status == "Pending").OrderBy(x => x.RequestedAt).Select(x => new InvitationDto(x.Id, x.OrganizationId, x.ProjectId, x.EmployeeProfileId, x.Email, x.DisplayName, x.SourceAccountId, x.Status, x.RequestedAt, x.DeliveryState)).ToListAsync(ct);
    }

    public async Task<ReviewInvitationResponse> ReviewAsync(Guid ownerId, Guid invitationId, bool approve, CancellationToken ct)
    {
        var organizationId = await _organizations.GetOrganizationIdAsync(ownerId, ct);
        var invitation = await _db.DeveloperInvitations.SingleOrDefaultAsync(x => x.Id == invitationId && x.OrganizationId == organizationId, ct) ?? throw new InvalidOperationException("Invitation was not found.");
        if (!approve)
        {
            invitation.Reject(ownerId); await _db.SaveChangesAsync(ct);
            return new(Map(invitation), null);
        }

        var token = invitation.Approve(ownerId, TimeSpan.FromHours(48));
        await _db.SaveChangesAsync(ct);
        await _projects.AddMemberAsync(invitation.ProjectId, invitation.EmployeeProfileId, invitation.OrganizationId, ct);
        var delivery = await _email.SendAsync(invitation.Email, invitation.DisplayName, token, ct);
        invitation.SetDelivery(delivery.State, delivery.Error);
        await _db.SaveChangesAsync(ct);
        var devUrl = !_email.Enabled && _environment.IsDevelopment() ? _email.BuildUrl(token) : null;
        return new(Map(invitation), devUrl);
    }

    public async Task<TokenInfoResponse?> GetTokenInfoAsync(string token, CancellationToken ct)
    {
        var hash = DeveloperInvitation.ComputeTokenHash(token);
        return await _db.DeveloperInvitations.AsNoTracking().Where(x => x.TokenHash == hash && x.Status == "Approved" && x.ExpiresAt > DateTimeOffset.UtcNow)
            .Select(x => new TokenInfoResponse(x.DisplayName, x.Email, x.ExpiresAt!.Value)).SingleOrDefaultAsync(ct);
    }

    public async Task<AcceptInvitationResponse> AcceptAsync(string token, string displayName, string password, CancellationToken ct)
    {
        var hash = DeveloperInvitation.ComputeTokenHash(token);
        var invitation = await _db.DeveloperInvitations.SingleOrDefaultAsync(x => x.TokenHash == hash, ct) ?? throw new InvalidOperationException("Invitation token is invalid.");
        if (invitation.Status == "Accepted" && invitation.ProvisionedUserId.HasValue) return new(invitation.ProvisionedUserId.Value, invitation.EmployeeProfileId, invitation.ProjectId);
        if (invitation.Status != "Approved" || invitation.ExpiresAt <= DateTimeOffset.UtcNow) throw new InvalidOperationException("Invitation token is expired or no longer active.");

        var userId = invitation.ProvisionedUserId ?? Guid.NewGuid();
        invitation.SetProvisionedUser(userId);
        await _db.SaveChangesAsync(ct);
        await _identity.ProvisionAsync(userId, invitation.Email, displayName, password, ct);
        await _talent.LinkAndActivateAsync(invitation.OrganizationId, invitation.EmployeeProfileId, userId, displayName, invitation.Email, ct);
        await _projects.AddMemberAsync(invitation.ProjectId, invitation.EmployeeProfileId, invitation.OrganizationId, ct);
        invitation.Accept();
        await _db.SaveChangesAsync(ct);
        return new(userId, invitation.EmployeeProfileId, invitation.ProjectId);
    }

    private static InvitationDto Map(DeveloperInvitation x) => new(x.Id, x.OrganizationId, x.ProjectId, x.EmployeeProfileId, x.Email, x.DisplayName, x.SourceAccountId, x.Status, x.RequestedAt, x.DeliveryState);
}

public sealed record CreateInvitationRequest(Guid ProjectId, Guid EmployeeProfileId, string SourceAccountId, string? Email);
public sealed record InvitationDto(Guid Id, Guid OrganizationId, Guid ProjectId, Guid EmployeeProfileId, string Email, string DisplayName, string SourceAccountId, string Status, DateTimeOffset RequestedAt, string DeliveryState);
public sealed record ReviewInvitationRequest(bool Approve);
public sealed record ReviewInvitationResponse(InvitationDto Invitation, string? DevelopmentInvitationUrl);
public sealed record TokenInfoResponse(string DisplayName, string Email, DateTimeOffset ExpiresAt);
public sealed record AcceptInvitationRequest(string Token, string DisplayName, string Password);
public sealed record AcceptInvitationResponse(Guid UserId, Guid EmployeeProfileId, Guid ProjectId);
