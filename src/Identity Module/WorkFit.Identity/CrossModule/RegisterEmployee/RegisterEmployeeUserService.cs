using Microsoft.AspNetCore.Identity;
using WorkFit.Identity.Contracts.Events;
using WorkFit.Identity.Contracts.IdentityServices;
using WorkFit.Identity.Domain.Entities;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.CrossModule.RegisterEmployee;

public sealed class RegisterEmployeeUserService : ICreateEmployeeUserService
{
    private readonly UserManager<WorkFitUser> _userManager;
    private readonly IMediator _mediator;

    public RegisterEmployeeUserService(UserManager<WorkFitUser> userManager,
            IMediator mediator
        )
    {
        _userManager = userManager;
        _mediator = mediator;
    }

    public async Task<EmployeeUserRegistrationResult> GetOrCreateAsync(
        string email,
        string displayName,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or whitespace.", nameof(password));

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            var existingRoles = await _userManager.GetRolesAsync(existingUser);
            if (!existingRoles.Contains("Employee", StringComparer.OrdinalIgnoreCase))
            {
                var roleResult = await _userManager.AddToRoleAsync(existingUser, "Employee");
                if (!roleResult.Succeeded)
                {
                    var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to assign Employee role: {roleErrors}");
                }
            }

            return new EmployeeUserRegistrationResult(
                existingUser.Id,
                existingUser.Email ?? email,
                false,
                null);
        }

        var user = new WorkFitUser(email, displayName);
        var createdUser = await _userManager.CreateAsync(user, password);
        if (!createdUser.Succeeded)
        {
            var errors = string.Join(", ", createdUser.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create employee user: {errors}");
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, "Employee");
        if (!addRoleResult.Succeeded)
        {
            var roleErrors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to assign Employee role: {roleErrors}");
        }

        await _mediator.Publish(
            new EmployeeRegisteredIntegrationEvent(user.Email ?? email, password),
            cancellationToken);

        return new EmployeeUserRegistrationResult(
            user.Id,
            user.Email ?? email,
            true,
            password);
    }
}
