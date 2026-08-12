using WorkFit.SharedKernel.MediatorContract;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace WorkFit.Documents.Features.Queries.GetDocumentById;

public sealed class GetDocumentByIdEndPoint : EndpointWithoutRequest<DocumentStreamResult>
{
    private readonly IMediator _mediator;

    public GetDocumentByIdEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/documents/{id:guid}");
        Options(x => x.WithTags("Document"));
        Roles("Employee");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var docId = Route<Guid>("id");
        var result = await _mediator.Send(new GetDocumentByIdQuery(docId), ct);

        await Send.StreamAsync(
            result.Content,
            contentType: result.ContentType,        // actual content type
            fileName: result.FileName,              // triggers correct extension
            cancellation: ct
        );
    }

}
