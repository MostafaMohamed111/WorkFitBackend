using Microsoft.AspNetCore.Identity;
using WorkFit.Identity.Domain.Exceptions;

namespace WorkFit.Identity.Domain.Entities;

public sealed class WorkFitUser : IdentityUser<Guid>
{

    public string  DisplayName { get; private set; } = string.Empty;

    private WorkFitUser()
    {
        
    }
    public WorkFitUser(string displayName)
    {
        if(string.IsNullOrEmpty(displayName)) throw new UserDisplayNameIsNullOrEmptyException();
        DisplayName = displayName;
    }
}
