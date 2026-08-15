namespace WorkFit.Documents.Contracts.TemporaryUploadService;

public interface ICreateTemporaryDocumentService
{
    Task<CreatedTemporaryDocumentDto> CreateAsync(
        Stream content,
        string fileName,
        string contentType,
        long size,
        Guid organizationId,
        CancellationToken ct = default);
}
