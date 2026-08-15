using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.WorkFlow.Features.UploadCvs;

public sealed class UploadCvsEndPoint : Endpoint<UploadCvsRequest, UploadCvsResponse>
{
    private readonly IMediator _mediator;

    public UploadCvsEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/workflow/cvs/upload");
        Roles("OrganizationOwner");
        AllowFileUploads();
        Options(x => x.WithTags("WorkFlow"));
    }

    public override async Task HandleAsync(UploadCvsRequest req, CancellationToken ct)
    {
        var command = new UploadCvsCommand(
            req.OrganizationId,
            req.Files ?? new List<Microsoft.AspNetCore.Http.IFormFile>());

        var result = await _mediator.Send(command, ct);
        await Send.OkAsync(result, cancellation: ct);
    }
}
