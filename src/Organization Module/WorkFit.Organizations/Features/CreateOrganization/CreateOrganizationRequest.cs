namespace WorkFit.Organizations.Features.CreateOrganization;

public sealed record CreateOrganizationRequest(
        string Name,
        Guid UserId
    );