using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.Organizations.Domain.Entities;

public sealed class Department : BaseEntity
{
    public string Name { get; private set; } = null!;
    public Guid OrganizationId { get; private set; }

    private Department() : base() { }

    private Department(string name, Guid organizationId) : base()
    {
        Name = name;
        OrganizationId = organizationId;
    }

    public static Department Create(string name, Guid organizationId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DepartmentNameIsNullOrEmptyException();
        return new Department(name, organizationId);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DepartmentNameIsNullOrEmptyException();
        Name = name;
        MarkUpdated();
    }
}
