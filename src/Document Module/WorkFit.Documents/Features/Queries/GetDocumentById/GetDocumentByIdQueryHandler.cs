using Microsoft.EntityFrameworkCore;
using IFileStorage = WorkFit.Documents.Infrastructure.Abstractions.IFileStorage;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.Documents.Domain.Entities;
using WorkFit.Documents.Features.Queries.Exceptions;
using WorkFit.Documents.Infrastructure.Data;

namespace WorkFit.Documents.Features.Queries.GetDocumentById;

public sealed class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, DocumentStreamResult>
{
    private readonly IFileStorage _fileStorage;
    private readonly DocumentDbContext _context;
    private readonly ICurrentUserContext _currentUser;

    public GetDocumentByIdQueryHandler(
        IFileStorage fileStorage,
        DocumentDbContext context,
        ICurrentUserContext currentUser
        )
    {
        _fileStorage = fileStorage;
        _context = context;
        _currentUser = currentUser;
    }
    public async Task<DocumentStreamResult> Handle(GetDocumentByIdQuery command, CancellationToken ct)
    {
        var document = await _context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == command.Id, ct);

        var userId = _currentUser.GetUserId();

        if (document is null)
            throw new EntityNotFoundException(
                    ModuleMarker.ModuleName,
                    nameof(Document),
                    command.Id
                );

        document.EnsureAttached();

        if (!document.IsAccessible(userId))
            throw new ForbiddenAccessException(
                ModuleMarker.ModuleName,
                nameof(Document),
                "You don't have access to this document.");

        var contentStream = await _fileStorage.OpenReadAsync(document.StorageKey, ct);

        if (contentStream is null)
            throw new DocumentContentNotFoundException(command.Id, document.StorageKey);

        return new DocumentStreamResult(
            
            Content: contentStream,
            ContentType: document.ContentType,
            FileName: document.FileName
        );

    }

}