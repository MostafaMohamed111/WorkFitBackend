using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace WorkFit.WorkFlow.Invitations;

public sealed class InvitationEmailOptions
{
    public bool Enabled { get; set; }
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
    private readonly InvitationEmailOptions _options;
    public InvitationEmailSender(IOptions<InvitationEmailOptions> options) => _options = options.Value;
    public bool Enabled => _options.Enabled;
    public string BuildUrl(string token) => $"{_options.FrontendBaseUrl.TrimEnd('/')}?token={Uri.EscapeDataString(token)}";

    public async Task<InvitationDeliveryResult> SendAsync(string email, string displayName, string token, CancellationToken cancellationToken)
    {
        if (!_options.Enabled) return new("Disabled", null);
        try
        {
            using var message = new MailMessage(_options.From, email)
            {
                Subject = "Your WorkFit developer invitation",
                Body = $"Hello {displayName},\n\nCreate your WorkFit account using this one-time link:\n{BuildUrl(token)}\n\nThis link expires in 48 hours."
            };
            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                Credentials = string.IsNullOrWhiteSpace(_options.Username) ? CredentialCache.DefaultNetworkCredentials : new NetworkCredential(_options.Username, _options.Password)
            };
            await client.SendMailAsync(message, cancellationToken);
            return new("Sent", null);
        }
        catch (Exception ex)
        {
            return new("Failed", ex.Message);
        }
    }
}
