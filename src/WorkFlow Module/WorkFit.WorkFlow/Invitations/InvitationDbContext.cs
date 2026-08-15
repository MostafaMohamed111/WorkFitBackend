using Microsoft.EntityFrameworkCore;

namespace WorkFit.WorkFlow.Invitations;

public sealed class InvitationDbContext : DbContext
{
    public InvitationDbContext(DbContextOptions<InvitationDbContext> options) : base(options) { }
    public DbSet<DeveloperInvitation> DeveloperInvitations => Set<DeveloperInvitation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("workflow");
        var invitation = builder.Entity<DeveloperInvitation>();
        invitation.ToTable("DeveloperInvitations");
        invitation.HasKey(x => x.Id);
        invitation.Property(x => x.Email).HasMaxLength(256).IsRequired();
        invitation.Property(x => x.DisplayName).HasMaxLength(255).IsRequired();
        invitation.Property(x => x.SourceSystem).HasMaxLength(50).IsRequired();
        invitation.Property(x => x.SourceAccountId).HasMaxLength(255).IsRequired();
        invitation.Property(x => x.Status).HasMaxLength(30).IsRequired();
        invitation.Property(x => x.TokenHash).HasMaxLength(64);
        invitation.Property(x => x.DeliveryState).HasMaxLength(30).IsRequired();
        invitation.Property(x => x.DeliveryError).HasMaxLength(2000);
        invitation.HasIndex(x => x.TokenHash).IsUnique().HasFilter("[TokenHash] IS NOT NULL");
        invitation.HasIndex(x => new { x.ProjectId, x.EmployeeProfileId }).IsUnique().HasFilter("[Status] IN ('Pending', 'Approved')");
        invitation.HasIndex(x => new { x.OrganizationId, x.Status });
    }
}
