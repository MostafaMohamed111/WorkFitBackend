namespace WorkFit.Email.Contracts;

public sealed record EmailMessage(
    string To,
    string Subject,
    string Body,
    bool IsBodyHtml = false);
