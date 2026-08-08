using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Engine.Contracts.CVParsing;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Engine.Features.Queries.GetBatchStatus;

public sealed class GetBatchStatusEndPoint : EndpointWithoutRequest<IReadOnlyList<CVParseJobStatusDto>>
{
    private readonly IMediator _mediator;

    public GetBatchStatusEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/cvparsing/batch/{BatchId:guid}");
        AllowAnonymous();
        Options(x => x.WithTags("CV Parsing"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var batchId = Route<Guid>("BatchId");
        var result = await _mediator.Send(new GetBatchStatusQuery(batchId), ct);
        await Send.OkAsync(result, cancellation: ct);
    }
}