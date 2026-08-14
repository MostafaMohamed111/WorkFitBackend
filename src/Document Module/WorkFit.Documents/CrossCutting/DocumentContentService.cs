using Microsoft.EntityFrameworkCore;
using WorkFit.Documents.Contracts.DocumentContentService;
using WorkFit.Documents.Domain.Entities;
using WorkFit.Documents.Infrastructure.Data;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using IFileStorage = WorkFit.Documents.Infrastructure.Abstractions.IFileStorage;

namespace WorkFit.Documents.CrossCutting;

public sealed class DocumentContentService : IDocumentContentService
{
    private readonly DocumentDbContext _context;
    private readonly IFileStorage _fileStorage;
    private readonly ICurrentUserContext _currentUser;

    public DocumentContentService(
        DocumentDbContext context,
        IFileStorage fileStorage,
        ICurrentUserContext currentUser)
    {
        _context = context;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    public async Task<DocumentContentDto> OpenReadAsync(Guid documentId, CancellationToken ct = default)
    {
        var document = await _context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == documentId, ct);

        if (document is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, nameof(Document), documentId);

        var currentUserId = _currentUser.GetUserId();
        var canReadTemporary = document.UploadedBy == currentUserId;
        var canReadAttached = document.DocumentStatus == DocumentStatus.Attached && document.IsAccessible(currentUserId);
        if (!canReadTemporary && !canReadAttached)
        {
            throw new ForbiddenAccessException(
                ModuleMarker.ModuleName,
                nameof(Document),
                "You don't have access to this document.");
        }

        var content = await _fileStorage.OpenReadAsync(document.StorageKey, ct);

        return new DocumentContentDto(
            document.Id,
            document.FileName,
            document.ContentType,
            document.Size,
            content);
    }
}
