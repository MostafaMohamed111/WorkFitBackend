using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using WorkFit.Email.Contracts;

namespace WorkFit.Email.Infrastructure;

internal sealed class SmtpEmailService(IOptions<SmtpOptions> options) : ISendEmailService
{
    private readonly SmtpOptions _options = options.Value;

    public async Task Send(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!_options.Enabled)
            throw new InvalidOperationException("Email sending is disabled.");

        using var mailMessage = new MailMessage(_options.From, message.To)
        {
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = message.IsBodyHtml
        };

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = new NetworkCredential(_options.Username, _options.Password)
        };

        await client.SendMailAsync(mailMessage, cancellationToken);
    }
}
