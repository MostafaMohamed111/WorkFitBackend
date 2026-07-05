using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.ProjectManagement.Domain.Entities;

public sealed class OrgDomain : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

 
    private OrgDomain(string name)
    {
        Name = name;
    }

    public static OrgDomain Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Domain name is required.", nameof(name));

        return new OrgDomain(name.Trim());
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Domain name is required.", nameof(name));

        Name = name.Trim();
    }
}