using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Integration.Contracts.IntegrationSyncService;
using WorkFit.Integration.Contracts.ProjectManagementProvider;
using WorkFit.Integration.Features.Shared;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Integration.Features.Commands.SyncIntegration;

public sealed class SyncIntegrationEndpoint : Endpoint<SyncIntegrationRequest, SyncResult>
{
    private readonly IMediator _mediator;

    public SyncIntegrationEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/integration/sync");
        Options(x => x.WithTags("Integration"));
    }

    public override async Task HandleAsync(SyncIntegrationRequest req, CancellationToken ct)
    {
        if (req.OrganizationId == Guid.Empty)
            AddError(r => r.OrganizationId, "OrganizationId must not be empty.");

        ThrowIfAnyErrors();

        var command = new SyncIntegrationCommand(
            req.OrganizationId
        );

        var result = await _mediator.Send(command, ct);
        await Send.OkAsync(result, ct);
    }
}

