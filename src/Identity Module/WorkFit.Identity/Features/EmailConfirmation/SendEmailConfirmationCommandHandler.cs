using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using WorkFit.Email.Contracts;
using WorkFit.Identity.Domain.Entities;
using WorkFit.Identity.Features.EmailConfirmation.Exceptions;
using WorkFit.Identity.Infrastructure.Email;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Features.EmailConfirmation;

public sealed class SendEmailConfirmationCommandHandler : IRequestHandler<SendEmailConfirmationCommand>
{
    private readonly UserManager<WorkFitUser> _userManager;
    private readonly ISendEmailService _emailService;
    private readonly IdentityEmailOptions _emailOptions;

    public SendEmailConfirmationCommandHandler(
        UserManager<WorkFitUser> userManager,
        ISendEmailService emailService,
        IOptions<IdentityEmailOptions> emailOptions)
    {
        _userManager = userManager;
        _emailService = emailService;
        _emailOptions = emailOptions.Value;
    }

    public async Task Handle(SendEmailConfirmationCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(command.Email)
            ?? throw new UserWithThisEmailDoesntExistException(command.Email);

        if (await _userManager.IsEmailConfirmedAsync(user))
            return;

        var email = user.Email ?? command.Email;
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = _emailOptions.BuildEmailConfirmationLink(email, token);

        var body = $"""
            Hello {user.DisplayName},

            Please confirm your email address by clicking the link below:

            {confirmationLink}

            If you did not create an account with WorkFit, you can safely ignore this email.

            WorkFit Team
            """;

        await _emailService.Send(
            new EmailMessage(email, "Confirm your WorkFit email", body),
            cancellationToken);
    }
}