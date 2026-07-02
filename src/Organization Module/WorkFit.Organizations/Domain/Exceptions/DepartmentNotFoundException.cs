using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Organizations.Domain.Exceptions;

public sealed class DepartmentNotFoundException : DomainException
{
    public DepartmentNotFoundException()
        : base("organization.department.not_found", "Department was not found.", "Department was not found.")
    {
    }
}
