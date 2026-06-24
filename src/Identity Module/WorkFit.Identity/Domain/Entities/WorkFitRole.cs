
using Microsoft.AspNetCore.Identity;
using WorkFit.Identity.Domain.Exceptions;

namespace WorkFit.Identity.Domain.Entities;

public sealed class WorkFitRole : IdentityRole<Guid>
{

    private WorkFitRole()
    {
        
    }
    public WorkFitRole(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new RoleNameIsNullOrEmptyException();
        Name = name;
    }
}
