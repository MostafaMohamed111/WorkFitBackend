using Microsoft.EntityFrameworkCore;
using WorkFit.Assessments.Contracts.IntegrationEvents;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.CrossCutting;
using WorkFit.TalentManagement.Domain.Enums;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.CrossCutting.EventHandlers;

internal record AssessmentApprovedIntegrationEventHandler : IIntegrationEventHandler<AssessmentApprovedIntegrationEvent>
{
    private readonly TalentDbContext _db;
    private readonly EmployeeIndexingStatePublisher _publisher;

    public AssessmentApprovedIntegrationEventHandler(TalentDbContext db, EmployeeIndexingStatePublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public async Task Handle(AssessmentApprovedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        var employee = await _db.EmployeeProfiles
            .Include(e => e.EmployeeSkills)
            .FirstOrDefaultAsync(e => e.Id == @event.EmployeeProfileId, cancellationToken)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName,
            "EmployeeProfile", @event.EmployeeProfileId);

        foreach (var change in @event.changes)
        {
            employee.AddOrUpdateEmployeeSkill(
                change.SkillId,
                @event.AssessmentId,
                change.SkillName,
                change.NewScore,
                change.evidence,
                "Assessment");
        }

        if (employee.Status != EmployeeProfileStatus.Inactive)
            employee.ActivateEmployee();

        await _db.SaveChangesAsync(cancellationToken);
        await _publisher.PublishAsync(employee.Id, "Activated", cancellationToken);
    }
}