using Microsoft.AspNetCore.Identity;
using WorkFit.Identity.Contracts.IdentityServices;
using WorkFit.Identity.CrossModule.RegisterOrganization.Exceptions;
using WorkFit.Identity.Domain.Entities;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.CrossModule.RegisterOrganization;

public sealed class RegisterOrganizationCommandHandler  : ICreateOrganizationUserService
{
    private readonly UserManager<WorkFitUser> _userManager;

    public RegisterOrganizationCommandHandler(
        UserManager<WorkFitUser> userManager
        )
    {
        _userManager = userManager;
    }
    public async Task<Guid> RegisterAsync(string email, string password, string confirmPassword, string organizationName, CancellationToken cancellationToken = default)
    {
        if (password != confirmPassword)
            throw new PasswordConfirmationMissMatchException();

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null) throw new EntityAlreadyExistsException(ModuleMarker.ModuleName, "WorkFitUser", existingUser.Id);
            

        var user = new WorkFitUser(email, organizationName);

        var createdUser = await _userManager.CreateAsync(user, password);

        if (!createdUser.Succeeded)
        {
            var errors = string.Join(", ", createdUser.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        await _userManager.AddToRoleAsync(user, "OrganizationOwner");

        return user.Id;
    }
}