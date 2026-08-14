using WorkFit.Documents.Contracts.TemporaryUploadService;
using WorkFit.Documents.Domain.Entities;
using WorkFit.Documents.Infrastructure.Data;
using WorkFit.SharedKernel.ICurrentUser;
using IFileStorage = WorkFit.Documents.Infrastructure.Abstractions.IFileStorage;

namespace WorkFit.Documents.CrossCutting;

public sealed class CreateTemporaryDocumentService : ICreateTemporaryDocumentService
{
    private readonly IFileStorage _fileStorage;
    private readonly DocumentDbContext _context;
    private readonly ICurrentUserContext _currentUser;

    public CreateTemporaryDocumentService(
        IFileStorage fileStorage,
        DocumentDbContext context,
        ICurrentUserContext currentUser)
    {
        _fileStorage = fileStorage;
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CreatedTemporaryDocumentDto> CreateAsync(
        Stream content,
        string fileName,
        string contentType,
        long size,
        Guid organizationId,
        CancellationToken ct = default)
    {
        var userId = _currentUser.GetUserId();

        var storageKey = await _fileStorage.UplaodDocumentAsync(
            content,
            fileName,
            contentType,
            ct);

        var document = Document.Create(
            userId,
            organizationId,
            storageKey,
            fileName,
            contentType,
            size);

        _context.Documents.Add(document);
        await _context.SaveChangesAsync(ct);

        return new CreatedTemporaryDocumentDto(
            document.Id,
            document.FileName,
            document.ContentType,
            document.Size);
    }
}
