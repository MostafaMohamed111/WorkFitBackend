namespace WorkFit.WorkFlow.Features.UploadCvs;

public sealed record UploadCvsResponse(
    int TotalDocuments,
    int Succeeded,
    int Failed,
    IReadOnlyList<UploadCvsItemResult> Items);

public sealed record UploadCvsItemResult(
    Guid? DocumentId,
    string FileName,
    bool Success,
    string? Error,
    Guid? IdentityUserId,
    Guid? EmployeeProfileId,
    Guid? AssessmentId,
    string? GeneratedPassword);
