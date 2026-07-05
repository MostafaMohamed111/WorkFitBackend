
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.WorkFlow.Features.OrganizationRegistration
{
    public sealed class OrganizationRegistrationEndPoint : Endpoint<OrganizationRegistrationRequest, Guid>
    {
        private readonly IMediator _mediator;

        public OrganizationRegistrationEndPoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/api/workflow/organization/register");
            Options(x => x.WithTags("WorkFlow"));
            AllowAnonymous();
        }

        public override async Task HandleAsync(OrganizationRegistrationRequest req, CancellationToken ct)
        {
            var command = new OrganizationRegistrationCommand(req.email, req.password, req.confirmPassword, req.organizationName);
            
            var organizationId = await _mediator.Send(command, ct);
            await Send.OkAsync(organizationId, cancellation: ct);
        }
    }
}
