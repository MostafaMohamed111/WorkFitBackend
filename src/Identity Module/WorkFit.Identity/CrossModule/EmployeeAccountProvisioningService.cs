using Microsoft.AspNetCore.Identity;
using WorkFit.Identity.Contracts.IdentityServices;
using WorkFit.Identity.Domain.Entities;

namespace WorkFit.Identity.CrossModule;

internal sealed class EmployeeAccountProvisioningService : IEmployeeAccountProvisioningService
{
    private readonly UserManager<WorkFitUser> _users;
    public EmployeeAccountProvisioningService(UserManager<WorkFitUser> users) => _users = users;

    public async Task ProvisionAsync(Guid userId, string email, string displayName, string password, CancellationToken cancellationToken = default)
    {
        var byId = await _users.FindByIdAsync(userId.ToString());
        if (byId is not null)
        {
            if (!string.Equals(byId.Email, email, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Provisioned invitation user conflicts with another account.");
            if (!await _users.IsInRoleAsync(byId, "Employee")) await _users.AddToRoleAsync(byId, "Employee");
            return;
        }

        if (await _users.FindByEmailAsync(email) is not null)
            throw new InvalidOperationException("An account with this email already exists.");

        var user = new WorkFitUser(userId, email.Trim(), displayName.Trim());
        var result = await _users.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(e => e.Description)));

        result = await _users.AddToRoleAsync(user, "Employee");
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(e => e.Description)));
    }
}
