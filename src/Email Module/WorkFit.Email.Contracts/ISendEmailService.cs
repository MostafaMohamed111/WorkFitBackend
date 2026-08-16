namespace WorkFit.Email.Contracts;

public interface ISendEmailService
{
    Task Send(EmailMessage message, CancellationToken cancellationToken = default);
}
