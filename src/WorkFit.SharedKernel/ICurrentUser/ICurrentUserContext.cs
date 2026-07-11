

namespace WorkFit.SharedKernel.ICurrentUser;

public interface ICurrentUserContext
{
    Guid GetOrganizationId(CancellationToken cancellationToken = default);
    IReadOnlyList<string> GetRoles(CancellationToken cancellationToken = default);
    Guid GetUserId(CancellationToken cancellationToken = default);
    string GetUserDisplayName(CancellationToken cancellationToken = default);
}
