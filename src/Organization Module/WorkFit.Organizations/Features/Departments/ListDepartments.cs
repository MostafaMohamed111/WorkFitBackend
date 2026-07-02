using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.Departments;

public sealed record ListDepartmentsRequest(Guid OrganizationId);
public sealed record ListDepartmentsQuery(Guid OrganizationId) : IRequest<IReadOnlyList<DepartmentResponse>>;

public sealed class ListDepartmentsQueryHandler : IRequestHandler<ListDepartmentsQuery, IReadOnlyList<DepartmentResponse>>
{
    private readonly OrganizationDbContext _context;

    public ListDepartmentsQueryHandler(OrganizationDbContext context) => _context = context;

    public async Task<IReadOnlyList<DepartmentResponse>> Handle(ListDepartmentsQuery request, CancellationToken cancellationToken = default)
    {
        return await _context.Departments.AsNoTracking()
            .Where(x => x.OrganizationId == request.OrganizationId)
            .OrderBy(x => x.Name)
            .Select(x => new DepartmentResponse(x.Id, x.OrganizationId, x.Name, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}

public sealed class ListDepartmentsEndpoint : Endpoint<ListDepartmentsRequest, IReadOnlyList<DepartmentResponse>>
{
    private readonly IMediator _mediator;

    public ListDepartmentsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/departments");
        AllowAnonymous();
        Options(x => x.WithTags("Organization"));
    }

    public override async Task HandleAsync(ListDepartmentsRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new ListDepartmentsQuery(req.OrganizationId), ct);
        await Send.OkAsync(response, ct);
    }
}
