using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Integration.Features.Commands.DeleteJiraSettings;

public sealed class DeleteJiraSettingsEndpoint : Endpoint<DeleteJiraSettingsRequest>
{
    private readonly IMediator _mediator;

    public DeleteJiraSettingsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("/api/integration/{OrganizationId}/jira-settings");
        Options(x => x.WithTags("Integration"));
    }

    public override async Task HandleAsync(DeleteJiraSettingsRequest req, CancellationToken ct)
    {
        if (req.OrganizationId == Guid.Empty)
        {
            AddError(r => r.OrganizationId, "OrganizationId must not be empty.");
            ThrowIfAnyErrors();
        }

        await _mediator.Send(new DeleteJiraSettingsCommand(req.OrganizationId), ct);
        await Send.NoContentAsync(ct);
    }
}
