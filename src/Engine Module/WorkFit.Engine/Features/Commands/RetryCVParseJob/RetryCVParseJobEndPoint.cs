using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Engine.Contracts.CVParsing;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Engine.Features.Commands.RetryCVParseJob;

public sealed class RetryCVParseJobEndPoint : EndpointWithoutRequest<UploadCVResponse>
{
    private readonly IMediator _mediator;

    public RetryCVParseJobEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/cvparsing/jobs/{JobId:guid}/retry");
        AllowAnonymous();
        Options(x => x.WithTags("CV Parsing"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var jobId = Route<Guid>("JobId");
        var result = await _mediator.Send(new RetryCVParseJobCommand(jobId), ct);
        await Send.OkAsync(result, cancellation: ct);
    }
}