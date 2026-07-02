using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.Departments;

public sealed record UpdateDepartmentRequest(Guid Id, string Name);
public sealed record UpdateDepartmentCommand(Guid Id, string Name) : IRequest<DepartmentResponse>;

public sealed class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, DepartmentResponse>
{
    private readonly OrganizationDbContext _context;

    public UpdateDepartmentCommandHandler(OrganizationDbContext context) => _context = context;

    public async Task<DepartmentResponse> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken = default)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new DepartmentNotFoundException();

        department.UpdateName(request.Name);
        await _context.SaveChangesAsync(cancellationToken);

        return new DepartmentResponse(department.Id, department.OrganizationId, department.Name, department.CreatedAt, department.UpdatedAt);
    }
}

public sealed class UpdateDepartmentEndpoint : Endpoint<UpdateDepartmentRequest, DepartmentResponse>
{
    private readonly IMediator _mediator;

    public UpdateDepartmentEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/departments/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateDepartmentRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new UpdateDepartmentCommand(req.Id, req.Name), ct);
        await Send.OkAsync(response, ct);
    }
}
