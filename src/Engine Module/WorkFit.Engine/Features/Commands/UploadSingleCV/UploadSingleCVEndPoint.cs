using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Engine.Contracts.CVParsing;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Engine.Features.Commands.UploadSingleCV;

public sealed class UploadSingleCVEndPoint : Endpoint<UploadSingleCVRequest, UploadCVResponse>
{
    private readonly IMediator _mediator;

    public UploadSingleCVEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/cvparsing");
        AllowAnonymous();
        AllowFileUploads();
        Options(x => x.WithTags("CV Parsing"));
    }

    public override async Task HandleAsync(UploadSingleCVRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new UploadSingleCVCommand(req.File), ct);
        await Send.OkAsync(result, cancellation: ct);
    }
}