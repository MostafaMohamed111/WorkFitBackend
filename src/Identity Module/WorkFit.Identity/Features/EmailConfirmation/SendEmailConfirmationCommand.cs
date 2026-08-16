using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Features.EmailConfirmation;

public sealed record SendEmailConfirmationCommand(string Email) : IRequest;