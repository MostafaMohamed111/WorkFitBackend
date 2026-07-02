using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.Departments;

public sealed record GetDepartmentRequest(Guid Id);
public sealed record GetDepartmentQuery(Guid Id) : IRequest<DepartmentResponse>;

public sealed class GetDepartmentQueryHandler : IRequestHandler<GetDepartmentQuery, DepartmentResponse>
{
    private readonly OrganizationDbContext _context;

    public GetDepartmentQueryHandler(OrganizationDbContext context) => _context = context;

    public async Task<DepartmentResponse> Handle(GetDepartmentQuery request, CancellationToken cancellationToken = default)
    {
        var department = await _context.Departments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new DepartmentNotFoundException();

        return new DepartmentResponse(department.Id, department.OrganizationId, department.Name, department.CreatedAt, department.UpdatedAt);
    }
}

public sealed class GetDepartmentEndpoint : Endpoint<GetDepartmentRequest, DepartmentResponse>
{
    private readonly IMediator _mediator;

    public GetDepartmentEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/departments/{id}");
        AllowAnonymous();
        Options(x => x.WithTags("Organization"));
    }

    public override async Task HandleAsync(GetDepartmentRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new GetDepartmentQuery(req.Id), ct);
        await Send.OkAsync(response, ct);
    }
}
