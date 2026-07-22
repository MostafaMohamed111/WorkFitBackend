

using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.ProjectManagement.Features.Exceptions;

internal sealed class UnAuthorizedTeamLeadAccessException(Guid id) : FeatureException(
    ModuleMarker.ModuleName,
    "UNAUTHORIZED_TEAM_LEAD_ACCESS",
    $"You are not authorized to access this resource as a team lead. Actor ID: {id}"
);
