using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Features.CreateAdmin;

public sealed class CreateAdminEndpoint : Endpoint<CreateAdminRequest>
{
    private readonly IMediator _mediator;

    public CreateAdminEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/identity/admins");
        Roles("SuperAdmin");
        Options(options => options.WithTags("Identity"));
    }

    public override async Task HandleAsync(CreateAdminRequest request, CancellationToken ct)
    {
        var command = new CreateAdminCommand(request.Email, request.Password, request.Name);
        await _mediator.Send(command, ct);
        await Send.NoContentAsync(ct);
    }
}
