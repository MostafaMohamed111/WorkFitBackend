
using Microsoft.AspNetCore.Identity;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Identity.Domain.Entities;

public sealed class WorkFitRole : IdentityRole<Guid>
{

    private WorkFitRole()
    {
        
    }
    public WorkFitRole(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new FeildIsNullOrEmptyException(ModuleMarker.ModuleName, "WorkFitRole", "Name");
        Name = name;
    }
}
