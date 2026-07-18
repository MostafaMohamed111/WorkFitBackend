using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.Organizations.Domain.Entities;

public sealed class OrganizationMember : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid UserId { get; private set; }

    private OrganizationMember() { } // EF

    private OrganizationMember(Guid organizationId, Guid userId)
    {
        OrganizationId = organizationId;
        UserId = userId;
    }

    internal static OrganizationMember Create(Guid organizationId, Guid userId)
        => new(organizationId, userId);
}