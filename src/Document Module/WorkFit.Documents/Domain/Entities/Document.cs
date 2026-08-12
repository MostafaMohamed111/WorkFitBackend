using WorkFit.SharedKernel.BaseEntity;
using WorkFit.Documents.Domain.Exceptions;

namespace WorkFit.Documents.Domain.Entities;

public sealed class Document : BaseEntity
{
    private static readonly HashSet<string> _allowedTypes =
     new(StringComparer.OrdinalIgnoreCase)
     {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/jpg",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document" // .docx
     };
    public Guid UploadedBy { get; private set; } = default!;
    public Guid OrganizationId { get; private set; }
    public string StorageKey { get; private set; } = default!;
    public string FileName { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public long Size { get; private set; }
    public DocumentStatus DocumentStatus { get; private set; }
    public DocumentAccessEntry? AccessEntry { get; private set; } 


    private Document() : base() // For EF Core
    {
    }

    private Document(Guid uploadedBy,
            Guid organizationId,
            string storageKey,
            string fileName,
            string contentType,
            long size
        ) : base()
    {
        UploadedBy = uploadedBy;
        OrganizationId = organizationId;
        StorageKey = storageKey;
        FileName = fileName;
        ContentType = contentType;
        Size = size;
        DocumentStatus = DocumentStatus.Temporary;

    }

    public static Document Create(
        Guid uploadedBy,
        Guid organizationId,
        string storageKey,
        string fileName,
        string contentType,
        long size
    )
    {
        // size validation
        if (size <= 0)
            throw new InvalidSizeDomainException();
        // content type validation
        if (!_allowedTypes.Contains(contentType))
            throw new InvalidContentTypeDomainException(contentType);


        var document = new Document(uploadedBy, organizationId, storageKey, fileName, contentType, size);
        return document;
    }

    public void MarkAsAttached()
    {
        if(DocumentStatus == DocumentStatus.Attached)
            throw new DocumentAlreadyAttachedDomainException();

        if(DocumentStatus == DocumentStatus.Deleted)
            throw new CannotAttachDeletedDocumentDomainException();

        DocumentStatus = DocumentStatus.Attached;
    }

    public void EnsureAttached()
    {
        if (DocumentStatus != DocumentStatus.Attached)
            throw new DocumentNotAttachedDomainException();
    }

    public void MarkAsDeleted()
        {
            if(DocumentStatus == DocumentStatus.Deleted)
                throw new DocumentAlreadyDeletedDomainException();
            if (DocumentStatus == DocumentStatus.Attached)
                throw new CannotDeleteAttachedDocumentDomainException();

            if (!CanBeDeleted())
                throw new DocumentTooYoungToBeDeletedDomainException();
        
            DocumentStatus = DocumentStatus.Deleted;
        }

    public void Detach()
    {
        if(DocumentStatus != DocumentStatus.Attached)
            throw new DocumentMustBeAttachedToDetachDomainException();
        
        DocumentStatus = DocumentStatus.Temporary;
    }

    private bool CanBeDeleted()
    {
        return CreatedAt <= DateTime.UtcNow.AddHours(-1);
    }

    public void GrantAccess(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new CannotGrantAccessToEmptyUserDomainException();
        if(AccessEntry != null)
            throw new DocumentAccessAlreadyGrantedDomainException();

        AccessEntry = new DocumentAccessEntry(userId);
    }

    public bool IsAccessible(Guid userId)
    {
        if (AccessEntry == null)
            return false;
        return AccessEntry.UserId == userId;
    }
}
