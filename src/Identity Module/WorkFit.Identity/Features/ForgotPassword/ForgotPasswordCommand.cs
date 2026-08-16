using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Features.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : IRequest;