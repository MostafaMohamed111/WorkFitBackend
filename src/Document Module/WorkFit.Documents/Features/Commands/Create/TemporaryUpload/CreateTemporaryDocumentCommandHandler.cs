using WorkFit.Documents.Domain.Entities;
using WorkFit.Documents.Infrastructure.Data;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using IFileStorage = WorkFit.Documents.Infrastructure.Abstractions.IFileStorage;

namespace WorkFit.Documents.Features.Commands.Create.TemporaryUpload;

public sealed class CreateTemporaryDocumentCommandHandler : IRequestHandler<CreateTemporaryDocumentCommand, DocumentCreationResponse>
{
    private readonly IFileStorage _fileStorage;
    private readonly DocumentDbContext _context;
    private readonly ICurrentUserContext _currentUser;

    public CreateTemporaryDocumentCommandHandler(IFileStorage fileStorage,
            DocumentDbContext context,
            ICurrentUserContext currentUser
        )
    {
        _fileStorage = fileStorage;
        _context = context;
        _currentUser = currentUser;
    }
    public async Task<DocumentCreationResponse> Handle(CreateTemporaryDocumentCommand command, CancellationToken ct)
    {
        var userId = _currentUser.GetUserId();

        var storageKey = await  _fileStorage.UplaodDocumentAsync(
                command.Content,
                command.FileName,
                command.ContentType,
                ct
            );

        var document = Document.Create(
            userId,
            command.OrganizationId,
            storageKey,
            command.FileName,
            command.ContentType,
            command.Size
        );

        _context.Documents.Add(document);

        await _context.SaveChangesAsync(ct);

        return new DocumentCreationResponse(
            document.Id, 
            document.FileName,
            document.ContentType);

    }
}