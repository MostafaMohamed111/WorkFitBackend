using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.Engine.Domain.Entities;

public sealed class CVParseJob : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid? BatchId { get; private set; }
    public string DocumentId { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string Mime { get; private set; } = string.Empty;
    public string FileHash { get; private set; } = string.Empty;
    public string? ExtractedText { get; private set; }
    public int? TokenUsage { get; private set; }
    public string Status { get; private set; } = "Queued";
    public string? Error { get; private set; }
    public int Attempts { get; private set; }
    public DateTime? HeartbeatAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid? EmployeeProfileId { get; private set; }
    public string? ParsedJson { get; private set; }

    private CVParseJob() { }

    public static CVParseJob Create(
        Guid organizationId,
        Guid? batchId,
        string documentId,
        string fileName,
        string mime,
        string fileHash)
    {
        return new CVParseJob
        {
            OrganizationId = organizationId,
            BatchId = batchId,
            DocumentId = documentId,
            FileName = fileName,
            Mime = mime,
            FileHash = fileHash
        };
    }

    public void SetExtractedText(string text) { ExtractedText = text; MarkUpdated(); }
    public void SetProcessing()
    {
        Status = "Processing";
        Attempts += 1;
        HeartbeatAt = DateTime.UtcNow;
        MarkUpdated();
    }
    public void Heartbeat() { HeartbeatAt = DateTime.UtcNow; }
    public void SetSucceeded(Guid employeeProfileId, string parsedJson, int? tokenUsage)
    {
        Status = "Succeeded";
        EmployeeProfileId = employeeProfileId;
        ParsedJson = parsedJson;
        TokenUsage = tokenUsage;
        CompletedAt = DateTime.UtcNow;
        MarkUpdated();
    }
    public void SetFailed(string status, string error)
    {
        Status = status;
        Error = error;
        CompletedAt = DateTime.UtcNow;
        MarkUpdated();
    }
    public void SetSkipped(Guid existingEmployeeProfileId)
    {
        Status = "SkippedDuplicate";
        EmployeeProfileId = existingEmployeeProfileId;
        CompletedAt = DateTime.UtcNow;
        MarkUpdated();
    }
    public void Requeue()
    {
        Status = "Queued";
        HeartbeatAt = null;
        MarkUpdated();
    }
}
