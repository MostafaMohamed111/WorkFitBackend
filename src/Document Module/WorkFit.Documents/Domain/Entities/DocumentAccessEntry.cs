namespace WorkFit.Documents.Domain.Entities;

public sealed record DocumentAccessEntry
{
    public Guid UserId { get; private set; }
    public DateTime GrantedAt { get; private set; }
    public DocumentAccessEntry(Guid userId)
    {
        UserId = userId;
        GrantedAt = DateTime.UtcNow;
    }
}