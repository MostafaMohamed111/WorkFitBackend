namespace WorkFit.Identity.Infrastructure.Email;

public sealed class IdentityEmailOptions
{
    public const string SectionName = "Identity:Email";

    public string FrontendBaseUrl { get; set; } = "http://localhost:4200";

    public string BuildEmailConfirmationLink(string email, string token) =>
        $"{FrontendBaseUrl.TrimEnd('/')}/confirm-email?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

    public string BuildPasswordResetLink(string email, string token) =>
        $"{FrontendBaseUrl.TrimEnd('/')}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
}