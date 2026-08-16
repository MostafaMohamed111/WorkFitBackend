using Microsoft.AspNetCore.Identity;
using WorkFit.Identity.Domain.Entities;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Features.CreateAdmin;

public sealed class CreateAdminCommandHandler : IRequestHandler<CreateAdminCommand>
{
    private const string AdminRole = "Admin";
    private readonly UserManager<WorkFitUser> _userManager;

    public CreateAdminCommandHandler(UserManager<WorkFitUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task Handle(CreateAdminCommand command, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(command.Email);
        if (existingUser is not null)
            throw new EntityAlreadyExistsException(ModuleMarker.ModuleName, nameof(WorkFitUser), existingUser.Id);

        var user = new WorkFitUser(command.Email, command.Name);
        var createResult = await _userManager.CreateAsync(user, command.Password);
        if (!createResult.Succeeded)
            throw CreateIdentityOperationException("create admin user", createResult);

        var roleResult = await _userManager.AddToRoleAsync(user, AdminRole);
        if (!roleResult.Succeeded)
            throw CreateIdentityOperationException($"assign {AdminRole} role", roleResult);
    }

    private static InvalidOperationException CreateIdentityOperationException(
        string operation,
        IdentityResult result)
    {
        var errors = string.Join(", ", result.Errors.Select(error => error.Description));
        return new InvalidOperationException($"Failed to {operation}: {errors}");
    }
}
