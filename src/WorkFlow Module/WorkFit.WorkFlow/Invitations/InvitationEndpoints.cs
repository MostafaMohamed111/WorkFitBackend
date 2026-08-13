using FastEndpoints;
using WorkFit.SharedKernel.ICurrentUser;

namespace WorkFit.WorkFlow.Invitations;

public sealed class CreateInvitationEndpoint : Endpoint<CreateInvitationRequest, InvitationDto>
{
    private readonly InvitationService _service;
    private readonly ICurrentUserContext _current;
    public CreateInvitationEndpoint(InvitationService service, ICurrentUserContext current) { _service = service; _current = current; }
    public override void Configure() { Post("/api/developer-invitations"); Roles("TeamLeader", "OrganizationOwner"); }
    public override async Task HandleAsync(CreateInvitationRequest req, CancellationToken ct) =>
        await Send.OkAsync(await _service.RequestAsync(_current.GetUserId(ct), _current.GetRoles(ct).Contains("OrganizationOwner"), req, ct), ct);
}

public sealed class ListPendingInvitationsEndpoint : EndpointWithoutRequest<IReadOnlyList<InvitationDto>>
{
    private readonly InvitationService _service; private readonly ICurrentUserContext _current;
    public ListPendingInvitationsEndpoint(InvitationService service, ICurrentUserContext current) { _service = service; _current = current; }
    public override void Configure() { Get("/api/developer-invitations/pending"); Roles("OrganizationOwner"); }
    public override async Task HandleAsync(CancellationToken ct) => await Send.OkAsync(await _service.ListPendingAsync(_current.GetUserId(ct), ct), ct);
}

public sealed class ReviewInvitationEndpoint : Endpoint<ReviewInvitationRequest, ReviewInvitationResponse>
{
    private readonly InvitationService _service; private readonly ICurrentUserContext _current;
    public ReviewInvitationEndpoint(InvitationService service, ICurrentUserContext current) { _service = service; _current = current; }
    public override void Configure() { Post("/api/developer-invitations/{invitationId:guid}/review"); Roles("OrganizationOwner"); }
    public override async Task HandleAsync(ReviewInvitationRequest req, CancellationToken ct) => await Send.OkAsync(await _service.ReviewAsync(_current.GetUserId(ct), Route<Guid>("invitationId"), req.Approve, ct), ct);
}

public sealed class InvitationTokenInfoEndpoint : EndpointWithoutRequest<TokenInfoResponse>
{
    private readonly InvitationService _service; public InvitationTokenInfoEndpoint(InvitationService service) => _service = service;
    public override void Configure() { Get("/api/developer-invitations/token/{token}"); AllowAnonymous(); }
    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _service.GetTokenInfoAsync(Route<string>("token")!, ct);
        if (result is null) await Send.NotFoundAsync(ct); else await Send.OkAsync(result, ct);
    }
}

public sealed class AcceptInvitationEndpoint : Endpoint<AcceptInvitationRequest, AcceptInvitationResponse>
{
    private readonly InvitationService _service; public AcceptInvitationEndpoint(InvitationService service) => _service = service;
    public override void Configure() { Post("/api/developer-invitations/accept"); AllowAnonymous(); }
    public override async Task HandleAsync(AcceptInvitationRequest req, CancellationToken ct) => await Send.OkAsync(await _service.AcceptAsync(req.Token, req.DisplayName, req.Password, ct), ct);
}
