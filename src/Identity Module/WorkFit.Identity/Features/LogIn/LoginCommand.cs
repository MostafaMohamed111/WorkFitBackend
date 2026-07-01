using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Features.LogIn;

public sealed record LoginCommand(string Email, string Password) : IRequest<string>;
