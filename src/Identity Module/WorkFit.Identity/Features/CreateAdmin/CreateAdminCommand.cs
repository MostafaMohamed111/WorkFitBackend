using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Features.CreateAdmin;

public sealed record CreateAdminCommand(
    string Email,
    string Password,
    string Name) : IRequest;
