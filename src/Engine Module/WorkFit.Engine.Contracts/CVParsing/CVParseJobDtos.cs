namespace WorkFit.Engine.Contracts.CVParsing;

public sealed record CVParseJobStatusDto(
    Guid JobId,
    Guid? BatchId,
    Guid? EmployeeProfileId,
    CVParseJobStatus Status,
    string? Error,
    int Attempts,
    int? TokenUsage,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public sealed record UploadCVResponse(Guid JobId, CVParseJobStatus Status, string? Message = null);

public sealed record UploadCVBulkResponse(
    Guid BatchId,
    int TotalFiles,
    int Accepted,
    IReadOnlyList<UploadCVResponse> Jobs);
