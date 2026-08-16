using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using WorkFit.Email.Contracts;

namespace WorkFit.WorkFlow.Invitations;

public sealed class InvitationEmailOptions
{
    public bool Enabled { get; set; } = true;
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string From { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string FrontendBaseUrl { get; set; } = "http://localhost:4200/invitations/accept";
}

public sealed record InvitationDeliveryResult(string State, string? Error);

public sealed class InvitationEmailSender
{
    private readonly ISendEmailService _emailService;
    private readonly InvitationEmailOptions _options;

    public InvitationEmailSender(ISendEmailService emailService, IOptions<InvitationEmailOptions> options)
    {
        _emailService = emailService;
        _options = options.Value;
    }

    public bool Enabled => true;
    public string BuildUrl(string token) => $"{(_options.FrontendBaseUrl ?? "http://localhost:4200/invitations/accept").TrimEnd('/')}?token={Uri.EscapeDataString(token)}";

    public async Task<InvitationDeliveryResult> SendAsync(string email, string displayName, string token, CancellationToken cancellationToken)
    {
        var acceptUrl = BuildUrl(token);
        var subject = "Your WorkFit Developer Invitation";
        var body = $@"Hello {displayName},

An organization owner has approved your developer invitation to join WorkFit.

Please click the link below to set up your account credentials and complete registration:
{acceptUrl}

This link is valid for 48 hours.

Best regards,
WorkFit Team";

        try
        {
            await _emailService.Send(new EmailMessage(email, subject, body, IsBodyHtml: false), cancellationToken);
            return new InvitationDeliveryResult("Sent", null);
        }
        catch (Exception ex)
        {
            // Direct SMTP Fallback if configured
            if (!string.IsNullOrWhiteSpace(_options.Host) && !string.IsNullOrWhiteSpace(_options.From))
            {
                try
                {
                    using var message = new MailMessage(_options.From, email)
                    {
                        Subject = subject,
                        Body = body
                    };
                    using var client = new SmtpClient(_options.Host, _options.Port)
                    {
                        EnableSsl = _options.EnableSsl,
                        Credentials = string.IsNullOrWhiteSpace(_options.Username) ? CredentialCache.DefaultNetworkCredentials : new NetworkCredential(_options.Username, _options.Password)
                    };
                    await client.SendMailAsync(message, cancellationToken);
                    return new InvitationDeliveryResult("Sent", null);
                }
                catch (Exception smtpEx)
                {
                    return new InvitationDeliveryResult("Failed", smtpEx.Message);
                }
            }

            return new InvitationDeliveryResult("Disabled", ex.Message);
        }
    }
}
