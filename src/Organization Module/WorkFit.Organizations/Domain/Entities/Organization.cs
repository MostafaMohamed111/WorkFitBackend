using WorkFit.SharedKernel.BaseEntity;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Organizations.Domain.Entities;

public sealed class Organization : BaseEntity
{
    public string Name { get; private set; } = null!;
    public Guid UserId { get; private set; }

    private Organization() : base() { } // EF
    private Organization(string name, Guid userId) : base()
    {
        Name = name;
        UserId = userId;
    }

    public static Organization Create(string name, Guid userId)
    {
        if(string.IsNullOrEmpty(name)) throw new FeildIsNullOrEmptyException(ModuleMarker.ModuleName, "Organization", "Name");
        return new Organization(name, userId);
    }
}
