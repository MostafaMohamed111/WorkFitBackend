using WorkFit.SharedKernel.Exceptions;

namespace WorkFit.Organizations.Domain.Exceptions;

public sealed class TeamNotFoundException : DomainException
{
    public TeamNotFoundException()
        : base("organization.team.not_found", "Team was not found.", "Team was not found.")
    {
    }
}
