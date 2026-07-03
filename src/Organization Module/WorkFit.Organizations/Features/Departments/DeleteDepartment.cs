using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.Departments;

public sealed record DeleteDepartmentRequest(Guid Id);
public sealed record DeleteDepartmentCommand(Guid Id) : IRequest;

public sealed class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand>
{
    private readonly OrganizationDbContext _context;

    public DeleteDepartmentCommandHandler(OrganizationDbContext context) => _context = context;

    public async Task Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken = default)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new DepartmentNotFoundException();

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public sealed class DeleteDepartmentEndpoint : Endpoint<DeleteDepartmentRequest>
{
    private readonly IMediator _mediator;

    public DeleteDepartmentEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("/api/departments/{id}");
        AllowAnonymous();
        Options(x => x.WithTags("Organization"));
    }

    public override async Task HandleAsync(DeleteDepartmentRequest req, CancellationToken ct)
    {
        await _mediator.Send(new DeleteDepartmentCommand(req.Id), ct);
        await Send.NoContentAsync(ct);
    }
}
