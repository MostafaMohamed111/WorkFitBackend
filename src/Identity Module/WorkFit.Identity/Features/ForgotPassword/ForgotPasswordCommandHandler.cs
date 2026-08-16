using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using WorkFit.Email.Contracts;
using WorkFit.Identity.Domain.Entities;
using WorkFit.Identity.Features.ForgotPassword.Exceptions;
using WorkFit.Identity.Infrastructure.Email;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Features.ForgotPassword;

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly UserManager<WorkFitUser> _userManager;
    private readonly ISendEmailService _emailService;
    private readonly IdentityEmailOptions _emailOptions;

    public ForgotPasswordCommandHandler(
        UserManager<WorkFitUser> userManager,
        ISendEmailService emailService,
        IOptions<IdentityEmailOptions> emailOptions)
    {
        _userManager = userManager;
        _emailService = emailService;
        _emailOptions = emailOptions.Value;
    }

    public async Task Handle(ForgotPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(command.Email)
            ?? throw new UserWithThisEmailDoesntExistException(command.Email);

        var email = user.Email ?? command.Email;
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = _emailOptions.BuildPasswordResetLink(email, token);

        var body = $"""
            Hello {user.DisplayName},

            We received a request to reset your WorkFit password.

            Click the link below to choose a new password:

            {resetLink}

            If you did not request a password reset, you can safely ignore this email.

            WorkFit Team
            """;

        await _emailService.Send(
            new EmailMessage(email, "Reset your WorkFit password", body),
            cancellationToken);
    }
}