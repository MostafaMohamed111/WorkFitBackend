

namespace WorkFit.SharedKernel.ICurrentUser;

public interface ICurrentUserContext
{
    Guid GetUserId(CancellationToken cancellationToken = default);
}
