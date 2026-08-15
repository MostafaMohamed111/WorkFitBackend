using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployees;

public sealed class GetEmployeesEndpoint
    : EndpointWithoutRequest<List<EmployeeListItemDto>>
{
    private readonly IMediator _mediator;

    public GetEmployeesEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/employees/{orgId}");
        Roles("TeamLeader", "OrganizationOwner");
        Options(x => x.WithTags("Talent Management"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orgId = Route<Guid>("orgId");
        var result = await _mediator.Send(new GetEmployeesQuery(orgId), ct);

        await Send.OkAsync(result, ct);
    }
}