using WorkFit.SharedKernel.Exceptions;

namespace WorkFit.Organizations.Domain.Exceptions;

public sealed class TeamNameIsNullOrEmptyException : DomainException
{
    public TeamNameIsNullOrEmptyException()
        : base("organization.team.name.empty", "Team name cannot be empty.", "Team name cannot be empty.")
    {
    }
}
