using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Engine.Contracts.CVParsing;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Engine.Features.Queries.GetCVParseJobStatus;

public sealed class GetCVParseJobStatusEndPoint : EndpointWithoutRequest<CVParseJobStatusDto>
{
    private readonly IMediator _mediator;

    public GetCVParseJobStatusEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/cvparsing/jobs/{JobId:guid}");
        AllowAnonymous();
        Options(x => x.WithTags("CV Parsing"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var jobId = Route<Guid>("JobId");
        var dto = await _mediator.Send(new GetCVParseJobStatusQuery(jobId), ct);
        await Send.OkAsync(dto, cancellation: ct);
    }
}