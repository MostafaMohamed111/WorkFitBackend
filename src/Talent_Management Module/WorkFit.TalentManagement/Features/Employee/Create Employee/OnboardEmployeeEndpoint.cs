using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.TalentManagement.Contracts.WriteServices.CreateEmployee;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.OnboardEmployee;

public sealed class OnboardEmployeeEndpoint
    : Endpoint<OnboardEmployeeRequest, OnboardEmployeeResponse>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUser;
    private readonly ICreateEmployeeService _createEmployeeService;

    public OnboardEmployeeEndpoint(
        IMediator mediator,
        ICurrentUserContext currentUser,
        ICreateEmployeeService createEmployeeService)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _createEmployeeService = createEmployeeService;
    }

    public override void Configure()
    {
        Post("/api/employees");
        Roles("Admin", "HR");
        Options(x => x.WithTags("Talent Management"));
    }

    public override async Task HandleAsync(OnboardEmployeeRequest req, CancellationToken ct)
    {
        var userId = _currentUser.GetUserId(ct);

        var employeeId = await _createEmployeeService.CreateEmployeeAsync(
            new EmployeeDetails(
                req.OrganizationId,
                userId,
                req.Email,
                req.Name,
                req.JobTitle,
                req.HireDate),
            ct);

        await Send.OkAsync(new OnboardEmployeeResponse(employeeId), ct);
    }
}
