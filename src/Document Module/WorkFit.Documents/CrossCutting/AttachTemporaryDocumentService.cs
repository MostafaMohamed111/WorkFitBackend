

using WorkFit.Documents.CrossCutting.Exceptions;
using WorkFit.Documents.Contracts;
using Microsoft.EntityFrameworkCore;
using WorkFit.Documents.Contracts.AttachDocumentService;
using WorkFit.Documents.Infrastructure.Data;
using WorkFit.SharedKernel.ICurrentUser;

namespace WorkFit.Documents.CrossCutting
{
    public sealed class AttachTemporaryDocumentService : IAttachTemporaryDocumentService
    {
        private readonly DocumentDbContext _context;
        private readonly ICurrentUserContext _currentUser;

        public AttachTemporaryDocumentService(DocumentDbContext context,
                ICurrentUserContext currentUser
            )
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<IReadOnlyDictionary<Guid, DocumentMetaDto>> AttachDocumentsByIdsAsync(
            IReadOnlyList<Guid> documentIds,
            Guid userId,
            CancellationToken ct
            )
        {
            // make sure the no one document is duplicated
            if (documentIds.Count != documentIds.Distinct().Count())
                throw new DuplicateDocumentIdsException();
            
                var documents = await _context.Documents.AsTracking()
                .Where(d => documentIds.Contains(d.Id)).ToListAsync(ct);

            if (documents.Count != documentIds.Count)
                throw new DocumentsUploadFailedException();

            if (documents.Any(d => d.UploadedBy != _currentUser.GetUserId()))
                throw new DocumentOwnershipMismatchException();

            var documentDtoList = new Dictionary<Guid, DocumentMetaDto>();

            foreach (var document in documents)
            {
                document.MarkAsAttached(); 
                document.GrantAccess(userId);
                documentDtoList[document.Id] = new DocumentMetaDto(
                        document.CreatedAt,
                        document.FileName,
                        document.ContentType,
                        document.Size
                    );
            }
            
            await _context.SaveChangesAsync(ct);

            return documentDtoList;
        }
    }
}
