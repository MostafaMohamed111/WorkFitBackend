
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Documents.Features.Commands.Create.TemporaryUpload;

public sealed class CreateTemporaryDocument : Endpoint<CreateDocumentRequest, DocumentCreationResponse>
{
    private readonly IMediator _mediator;

    public CreateTemporaryDocument(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("api/documents");
        Roles("Employee", "OrganizationOwner");
        Options(x => x.WithTags("Document"));
        AllowFileUploads();
    }

    public override async Task HandleAsync(CreateDocumentRequest req, CancellationToken ct)
    {

        // stream, file name, content type, user id
        var command = new CreateTemporaryDocumentCommand(
            req.File.OpenReadStream(),
            req.File.FileName,
            req.File.ContentType,
            req.File.Length,
            req.OrganizationId
            );
        Response = await _mediator.Send(command, ct);
        await Send.OkAsync(Response);
    }
}
