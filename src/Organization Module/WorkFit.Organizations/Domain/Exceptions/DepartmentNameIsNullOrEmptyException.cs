using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Organizations.Domain.Exceptions;

public sealed class DepartmentNameIsNullOrEmptyException : DomainException
{
    public DepartmentNameIsNullOrEmptyException()
        : base("organization.department.name.empty", "Department name cannot be empty.", "Department name cannot be empty.")
    {
    }
}
