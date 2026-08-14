using WorkFit.Documents.Contracts.TemporaryUploadService;
using WorkFit.Documents.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Documents.Features.Commands.Create.TemporaryUpload;

public sealed class CreateTemporaryDocumentCommandHandler : IRequestHandler<CreateTemporaryDocumentCommand, DocumentCreationResponse>
{
    private readonly ICreateTemporaryDocumentService _createTemporaryDocumentService;

    public CreateTemporaryDocumentCommandHandler(
        ICreateTemporaryDocumentService createTemporaryDocumentService)
    {
        _createTemporaryDocumentService = createTemporaryDocumentService;
    }

    public async Task<DocumentCreationResponse> Handle(CreateTemporaryDocumentCommand command, CancellationToken ct)
    {
        var document = await _createTemporaryDocumentService.CreateAsync(
            command.Content,
            command.FileName,
            command.ContentType,
            command.Size,
            command.OrganizationId,
            ct);

        return new DocumentCreationResponse(
            document.Id,
            document.FileName,
            document.ContentType);

    }
}