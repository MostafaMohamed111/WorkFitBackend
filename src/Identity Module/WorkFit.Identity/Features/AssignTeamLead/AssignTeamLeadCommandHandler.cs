using Microsoft.AspNetCore.Identity;
using WorkFit.Identity.Domain.Entities;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Features.AssignTeamLead;

public sealed class AssignTeamLeadCommandHandler : IRequestHandler<AssignTeamLeadCommand>
{
    private const string TeamLeadRole = "TeamLead";
    private readonly UserManager<WorkFitUser> _userManager;

    public AssignTeamLeadCommandHandler(UserManager<WorkFitUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task Handle(AssignTeamLeadCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString())
            ?? throw new EntityNotFoundException(
                ModuleMarker.ModuleName,
                nameof(WorkFitUser),
                command.UserId);

        if (await _userManager.IsInRoleAsync(user, TeamLeadRole))
            return;

        var roleResult = await _userManager.AddToRoleAsync(user, TeamLeadRole);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed to assign {TeamLeadRole} role: {errors}");
        }
    }
}
