using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using WorkFit.Organizations.Contracts.OrganizationServices;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.TalentManagement.Contracts.LookUpServices;

namespace WorkFit.Organizations.Features.OrganizationsMe;

public sealed record GetOrganizationIdRequest(Guid UserId);

public sealed class GetOrganizationIdEndpoint : Endpoint<GetOrganizationIdRequest, Guid>
{
    private readonly OrganizationDbContext _context;
    private readonly IEmployeeLookUpService _employeeLookUpService;

    public GetOrganizationIdEndpoint(
        OrganizationDbContext context,
        IEmployeeLookUpService employeeLookUpService)
    {
        _context = context;
        _employeeLookUpService = employeeLookUpService;
    }

    public override void Configure()
    {
        Get("/api/organizations/me/id");
        AllowAnonymous();
        Options(x => x.WithTags("Organization"));
    }

    public override async Task HandleAsync(GetOrganizationIdRequest req, CancellationToken ct)
    {
        var employee = await _employeeLookUpService.GetEmployeeByUserIdAsync(req.UserId, ct);
        if (employee is not null)
        {
            await Send.OkAsync(employee.OrganizationId, ct);
            return;
        }

        var organization = await _context.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == req.UserId, ct)
            ?? throw new OrganizationNotFoundException();

        var organizationId = organization.Id;
        await Send.OkAsync(organizationId, ct);
    }
}
