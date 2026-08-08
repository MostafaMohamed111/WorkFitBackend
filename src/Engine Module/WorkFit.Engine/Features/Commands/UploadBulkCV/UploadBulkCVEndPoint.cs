using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Engine.Contracts.CVParsing;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Engine.Features.Commands.UploadBulkCV;

public sealed class UploadBulkCVEndPoint : Endpoint<UploadBulkCVRequest, UploadCVBulkResponse>
{
    private readonly IMediator _mediator;

    public UploadBulkCVEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/cvparsing/bulk");
        AllowAnonymous();
        AllowFileUploads();
        Options(x => x.WithTags("CV Parsing"));
    }

    public override async Task HandleAsync(UploadBulkCVRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new UploadBulkCVCommand(req.Zip), ct);
        await Send.OkAsync(result, cancellation: ct);
    }
}