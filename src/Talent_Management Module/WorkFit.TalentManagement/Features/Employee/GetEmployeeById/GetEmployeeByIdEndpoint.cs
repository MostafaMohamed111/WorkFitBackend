using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployeeById;

public sealed class GetEmployeeByUserIdEndPoint : EndpointWithoutRequest<EmployeeDetailsDto>
{
    private readonly IMediator _mediator;

    public GetEmployeeByUserIdEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/employees/{id}", "/api/talent-management/employees/{id}");
        Roles("TeamLeader", "OrganizationOwner", "Admin", "SuperAdmin", "Employee");
        Options(x => x.WithTags("Talent Management"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var idRoute = Route<string>("id");

        if (string.IsNullOrEmpty(idRoute) || !Guid.TryParse(idRoute, out var targetId))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        try
        {
            var query = new GetEmployeeByUserIdCommand(targetId);
            var result = await _mediator.Send(query, ct);
            await Send.OkAsync(result, cancellation: ct);
        }
        catch (EntityNotFoundException)
        {
            await Send.NotFoundAsync(ct);
        }
        catch (ForbiddenAccessException)
        {
            await Send.ForbiddenAsync(ct);
        }
    }
}