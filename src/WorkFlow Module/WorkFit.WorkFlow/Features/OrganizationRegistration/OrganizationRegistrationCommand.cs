using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.WorkFlow.Features.OrganizationRegistration
{
    public sealed record OrganizationRegistrationCommand(
            string Email,
            string Password,
            string ConfirmPassword,
            string OrganizationName
        ) : IRequest<Guid>;
}
