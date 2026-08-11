
using WorkFit.Documents.Contracts;
using WorkFit.Documents.Contracts.DocumentLookUpService;
using WorkFit.Documents.CrossCutting.Exceptions;
using WorkFit.Documents.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using WorkFit.SharedKernel.ICurrentUser;

namespace WorkFit.Documents.CrossCutting;

public sealed class DocumentLookUpService : IDocumentLookUpService
{
    private readonly DocumentDbContext _context;
    private readonly ICurrentUserContext _currentUser;

    public DocumentLookUpService(DocumentDbContext context,
            ICurrentUserContext currentUser
        )
    {
        _context = context;
        _currentUser = currentUser;
    }
    public async Task<IReadOnlyList<DocumentMetaDto>> GetDocumentsByIdsAsync(IReadOnlyList<Guid> documentIds, CancellationToken ct)
    {
        var documents = await _context.Documents.AsNoTracking().Where(d => documentIds.Contains(d.Id)).ToListAsync();

        if (documents.Count != documentIds.Count)
            throw new DocumentsNotFoundException();

        var result = new List<DocumentMetaDto>();

        foreach (var document in documents)
        {
            result.Add(new DocumentMetaDto(
                document.CreatedAt,
                document.FileName,
                document.ContentType,
                document.Size
            ));
        }

        return result;
    }
}
