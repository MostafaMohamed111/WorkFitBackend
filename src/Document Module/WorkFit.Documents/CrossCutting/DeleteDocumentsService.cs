

using WorkFit.Documents.Contracts;
using WorkFit.Documents.Infrastructure.Abstractions;
using WorkFit.Documents.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace WorkFit.Documents.CrossCutting;

internal sealed class DeleteDocumentsService : IDeleteDocumentService
{
    private readonly DocumentDbContext _context;
    private readonly IFileStorage _fileStorage;

    public DeleteDocumentsService(DocumentDbContext context,
            IFileStorage fileStorage
        )
    {
        _context = context;
        _fileStorage = fileStorage;
    }
    public async Task DeleteDocumentsAsync(List<Guid> documentIds, CancellationToken ct)
    {
        var documents = await _context.Documents
            .Where(d => documentIds.Contains(d.Id)).ToListAsync(ct);

        _context.Documents.RemoveRange(documents);

        foreach (var document in documents) 
            await _fileStorage.DeleteAsync(document.StorageKey, ct);

        await _context.SaveChangesAsync();
    }
}
