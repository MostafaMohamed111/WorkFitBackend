using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.TalentManagement.Domain.Entities;

public class PreferredDomain : BaseEntity
{ 
    public Guid EmployeeId { get; private set; }
    public string DomainName { get; private set; } = default!;
    public int Order { get; private set; } // 1 = أعلى أولوية

    public Employee Employee { get; private set; } = default!;

    public static PreferredDomain Create(Guid empId, string name, int order) => new()
    {
        EmployeeId = empId,
        DomainName = name,
        Order = order
    };

    public void SetOrder(int newOrder) => Order = newOrder;
}