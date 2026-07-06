

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkFit.Identity.Domain.Entities;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Features.RegisterWithRoleForTesting;

public sealed class RegisterWithRoleForTestingCommandHandler : IRequestHandler<RegisterWithRoleForTestingCommand>
{
    private readonly UserManager<WorkFitUser> _userManager;
    private readonly RoleManager<WorkFitRole> _roleManager;

    public RegisterWithRoleForTestingCommandHandler(UserManager<WorkFitUser> userManager,
            RoleManager<WorkFitRole> roleManager
        )
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }
    public async Task Handle(RegisterWithRoleForTestingCommand command, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(command.Email);

        if (existingUser != null)
            throw new InvalidOperationException("User exists");

        var user = new WorkFitUser(command.Email, command.Name);

        var existingRoles = await _roleManager.Roles.ToListAsync(cancellationToken);

        foreach (var role in command.Roles)
        {
            if (!existingRoles.Any(r => r.NormalizedName == role.ToUpper()))
            {
                throw new InvalidOperationException($"Role '{role}' does not exist.");
            }
        }

        var result = await _userManager.CreateAsync(user, command.Password);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Failed to create user.");
        }

        foreach (var role in command.Roles)
        {
            await _userManager.AddToRoleAsync(user, role);
        }
    }
}