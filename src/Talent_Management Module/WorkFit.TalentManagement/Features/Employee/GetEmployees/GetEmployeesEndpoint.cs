using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployees;

public sealed class GetEmployeesEndpoint
    : EndpointWithoutRequest<List<EmployeeListItemDto>>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly TalentDbContext _context;

    public GetEmployeesEndpoint(
        IMediator mediator,
        ICurrentUserContext currentUserContext,
        TalentDbContext context)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
        _context = context;
    }

    public override void Configure()
    {
        Get("/api/employees/");
        Roles("TeamLeader", "OrganizationOwner");
        Options(x => x.WithTags("Talent Management"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        


        var result = await _mediator.Send(new GetEmployeesQuery(), ct);

        await Send.OkAsync(result, ct);
    }
}