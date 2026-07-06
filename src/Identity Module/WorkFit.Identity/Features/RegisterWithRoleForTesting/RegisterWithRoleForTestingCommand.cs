

using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Features.RegisterWithRoleForTesting;

public sealed record RegisterWithRoleForTestingCommand(
        string Email,
        string Password,
        List<string> Roles,
        string Name
    ) : IRequest;
