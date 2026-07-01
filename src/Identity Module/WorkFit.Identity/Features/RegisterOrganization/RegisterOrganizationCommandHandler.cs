using Microsoft.AspNetCore.Identity;
using WorkFit.Identity.Contracts.IntegrationEvents.OrganizationRegistered;
using WorkFit.Identity.Domain.Entities;
using WorkFit.Identity.Features.RegisterOrganization.Exceptions;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Features.RegisterOrganization;

public sealed class RegisterOrganizationCommandHandler : IRequestHandler<RegisterOrganizationCommand>
{
    private readonly UserManager<WorkFitUser> _userManager;
    private readonly IMediator _mediator;

    public RegisterOrganizationCommandHandler(
        UserManager<WorkFitUser> userManager,
        IMediator mediator
        )
    {
        _userManager = userManager;
        _mediator = mediator;
    }
    public async Task Handle(RegisterOrganizationCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Password != command.ConfirmPassword)
            throw new PasswordConfirmationMissMatchException();

        var existingUser = await _userManager.FindByEmailAsync(command.Email);
        if (existingUser != null) throw new EntityAlreadyExistsException(ModuleMarker.ModuleName, "WorkFitUser", existingUser.Id);
            

        var user = new WorkFitUser(command.OrganizationName, command.Email, command.OrganizationName);

        var createdUser = await _userManager.CreateAsync(user, command.Password);

        if (!createdUser.Succeeded)
        {
            var errors = string.Join(", ", createdUser.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        await _mediator.Publish(new OrganizationRegisteredIntegrationEvent(user.Id, command.Email), cancellationToken);
    }
}