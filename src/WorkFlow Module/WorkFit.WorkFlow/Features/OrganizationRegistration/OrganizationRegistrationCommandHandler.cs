using WorkFit.Identity.Contracts.IdentityServices;
using WorkFit.Organizations.Contracts.OrganizationServices;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.WorkFlow.Features.OrganizationRegistration;

public sealed class OrganizationRegistrationCommandHandler : IRequestHandler<OrganizationRegistrationCommand, Guid>
{

    private readonly ICreateOrganizationUserService _createOrganizationUserService;

    private readonly ICreateOrganizationService _createOrganizationService;

    public OrganizationRegistrationCommandHandler(
        ICreateOrganizationUserService createOrganizationUserService,
        ICreateOrganizationService createOrganizationService
        )
    {
        _createOrganizationUserService = createOrganizationUserService;
        _createOrganizationService = createOrganizationService;
    }
    public async Task<Guid> Handle(OrganizationRegistrationCommand request, CancellationToken cancellationToken = default)
    {
        // coordinating the creation of organization and organization identity user

        // creating organization identity user 
        var userId = await _createOrganizationUserService.RegisterAsync(request.Email, request.Password, request.ConfirmPassword, request.OrganizationName, cancellationToken);
        // creating organization
        var organizationId = await _createOrganizationService.CreateAsync(request.OrganizationName, userId, cancellationToken);
        // compensation logic can be added here if needed, for example, if organization creation fails, we might want to delete the created user.

        return organizationId;
    }
}