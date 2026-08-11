
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Documents.Features.Commands.Create.TemporaryUpload;

public sealed record CreateTemporaryDocumentCommand(
        Stream Content,
        string FileName,
        string ContentType,
        long Size,
        Guid OrganizationId

    ) : IRequest<DocumentCreationResponse>;
