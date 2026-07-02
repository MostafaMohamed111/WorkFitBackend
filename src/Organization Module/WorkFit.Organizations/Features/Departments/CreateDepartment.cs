using FastEndpoints;
using WorkFit.Organizations.Domain.Entities;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.Departments;

public sealed record CreateDepartmentRequest(Guid OrganizationId, string Name);
public sealed record CreateDepartmentCommand(Guid OrganizationId, string Name) : IRequest<DepartmentResponse>;

public sealed class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, DepartmentResponse>
{
    private readonly OrganizationDbContext _context;

    public CreateDepartmentCommandHandler(OrganizationDbContext context) => _context = context;

    public async Task<DepartmentResponse> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken = default)
    {
        var department = Department.Create(request.Name, request.OrganizationId);
        _context.Departments.Add(department);
        await _context.SaveChangesAsync(cancellationToken);

        return new DepartmentResponse(department.Id, department.OrganizationId, department.Name, department.CreatedAt, department.UpdatedAt);
    }
}

public sealed class CreateDepartmentEndpoint : Endpoint<CreateDepartmentRequest, DepartmentResponse>
{
    private readonly IMediator _mediator;

    public CreateDepartmentEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/departments");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateDepartmentRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new CreateDepartmentCommand(req.OrganizationId, req.Name), ct);
        await Send.OkAsync(response, ct);
    }
}
