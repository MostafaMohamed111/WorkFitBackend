using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.Organizations.Domain.Entities;

public sealed class Team : BaseEntity
{
    public string Name { get; private set; } = null!;
    public Guid DepartmentId { get; private set; }
    public Guid? LeadUserId { get; private set; }

    private Team() : base() { }

    private Team(string name, Guid departmentId) : base()
    {
        Name = name;
        DepartmentId = departmentId;
    }

    public static Team Create(string name, Guid departmentId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new TeamNameIsNullOrEmptyException();
        return new Team(name, departmentId);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new TeamNameIsNullOrEmptyException();
        Name = name;
        MarkUpdated();
    }

    public void AssignLead(Guid? leadUserId)
    {
        LeadUserId = leadUserId;
        MarkUpdated();
    }
}
