using Microsoft.AspNetCore.Identity;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Identity.Domain.Entities;

public sealed class WorkFitUser : IdentityUser<Guid>
{

    public string  DisplayName { get; private set; } = string.Empty;

    private WorkFitUser()
    {
        
    }


    public WorkFitUser(string email, string displayName)
    {
        if(string.IsNullOrEmpty(displayName)) throw new FeildIsNullOrEmptyException(ModuleMarker.ModuleName, "WorkFitUser", "DisplayName");
        if(string.IsNullOrEmpty(email)) throw new FeildIsNullOrEmptyException(ModuleMarker.ModuleName, "WorkFitUser", "Email");
        DisplayName = displayName;
        Email = email;
        UserName = email;    
    }
}
