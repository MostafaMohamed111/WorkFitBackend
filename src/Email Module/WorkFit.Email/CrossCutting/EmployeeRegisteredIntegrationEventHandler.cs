using WorkFit.Email.Contracts;
using WorkFit.Identity.Contracts.Events;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Email.CrossCutting;

internal sealed class EmployeeRegisteredIntegrationEventHandler(ISendEmailService emailService)
    : IIntegrationEventHandler<EmployeeRegisteredIntegrationEvent>
{
    public Task Handle(
        EmployeeRegisteredIntegrationEvent @event,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var body = $"""
            Welcome to WorkFit!

            Your employee account has been created successfully.

            Email: {@event.email}
            Password: {@event.password}

            Please sign in and change your password as soon as possible.

            WorkFit Team
            """;

        return emailService.Send(
            new EmailMessage(
                @event.email,
                "Welcome to WorkFit",
                body),
            cancellationToken);
    }
}
