

namespace WorkFit.SharedKernel.ICurrentUser;

public interface ICurrentUserContext
{
    IReadOnlyList<string> GetRoles(CancellationToken cancellationToken = default);
    Guid GetUserId(CancellationToken cancellationToken = default);
    string GetUserDisplayName(CancellationToken cancellationToken = default);
}
