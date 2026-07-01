using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Features.RegisterOrganization;

public sealed record RegisterOrganizationCommand(string Email,
    string Password,
    string ConfirmPassword,
    string OrganizationName) : IRequest;
