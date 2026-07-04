using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.TalentManagement.Domain.Entities;

public class WorkHistory : BaseEntity
{ 
    public Guid EmployeeId { get; private set; }
    public Guid ProjectId { get; private set; } // reference to Module 5
    public string ProjectName { get; private set; } = default!; // cached
    public string Role { get; private set; } = default!;
    public DateTime From { get; private set; }
    public DateTime? To { get; private set; } // null = لسه شغال

    public Employee Employee { get; private set; } = default!;

    public static WorkHistory Create(Guid empId, Guid projectId,
        string projectName, string role, DateTime from) => new()
        {
            EmployeeId = empId,
            ProjectId = projectId,
            ProjectName = projectName,
            Role = role,
            From = from
        };

    public void Close(DateTime endDate) => To = endDate;
}