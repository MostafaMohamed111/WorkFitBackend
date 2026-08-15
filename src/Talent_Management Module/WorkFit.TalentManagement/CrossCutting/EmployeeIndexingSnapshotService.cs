using Microsoft.EntityFrameworkCore;
using WorkFit.TalentManagement.Contracts.Indexing;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.CrossCutting;

internal sealed class EmployeeIndexingSnapshotService : IEmployeeIndexingSnapshotService
{
    private readonly TalentDbContext _db;

    public EmployeeIndexingSnapshotService(TalentDbContext db) => _db = db;

    public async Task<EmployeeIndexingSnapshot?> GetEmployeeAsync(
        Guid employeeProfileId,
        CancellationToken cancellationToken = default)
    {
        var employees = await LoadEmployees()
            .Where(x => x.Id == employeeProfileId)
            .ToListAsync(cancellationToken);

        return employees.Count == 0
            ? null
            : await BuildSnapshotAsync(employees[0], cancellationToken);
    }

    public async Task<IReadOnlyList<EmployeeIndexingSnapshot>> GetOrganizationEmployeesAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var query = LoadEmployees();
        if (organizationId != Guid.Empty)
        {
            query = query.Where(x => x.OrganizationId == organizationId);
        }

        var employees = await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
        var snapshots = new List<EmployeeIndexingSnapshot>(employees.Count);

        foreach (var employee in employees)
            snapshots.Add(await BuildSnapshotAsync(employee, cancellationToken));

        return snapshots;
    }

    private IQueryable<Domain.Entities.EmployeeProfile> LoadEmployees() =>
        _db.EmployeeProfiles
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.EmployeeSkills)
                .ThenInclude(x => x.ConfidenceChanges)
                .ThenInclude(x => x.ConfidenceEvidences)
            .Include(x => x.Certifications);

    private async Task<EmployeeIndexingSnapshot> BuildSnapshotAsync(
        Domain.Entities.EmployeeProfile employee,
        CancellationToken cancellationToken)
    {
        var taskRows = await _db.TaskAllocations
            .AsNoTracking()
            .Where(x => x.EmployeeProfileId == employee.Id && !x.IsDeleted)
            .ToListAsync(cancellationToken);

        var completedRows = taskRows.Where(x => x.CompletedAt.HasValue || x.Status == "Done").ToArray();
        var taskPerformance = taskRows.Count == 0
            ? null
            : new EmployeeTaskPerformanceIndexingSnapshot(
                taskRows.Count,
                completedRows.Length,
                completedRows.Sum(x => x.StoryPoints ?? 0),
                completedRows.Max(x => x.CompletedAt));

        return new EmployeeIndexingSnapshot(
            employee.Id,
            employee.OrganizationId,
            employee.Name,
            employee.JobTitle,
            employee.Bio,
            employee.Status.ToString(),
            employee.CurrentAllocationPercentage,
            employee.HireDate,
            employee.EmployeeSkills
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.SkillName)
                .Select(x => new EmployeeSkillIndexingSnapshot(
                    x.SkillId,
                    x.SkillName,
                    x.ConfidenceScore,
                    x.ConfidenceChanges
                        .Where(c => !c.IsDeleted)
                        .SelectMany(c => c.ConfidenceEvidences)
                        .Where(e => !e.IsDeleted)
                        .OrderByDescending(e => e.EvidenceDate)
                        .Select(e => new EmployeeSkillEvidenceIndexingSnapshot(e.Source, e.Details, e.EvidenceDate))
                        .ToArray()))
                .ToArray(),
            employee.Certifications
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.IssueDate)
                .Select(x => new EmployeeCertificationIndexingSnapshot(
                    x.Name, x.IssuingOrganization, x.IssueDate, x.ExpiryDate, x.IsExpired))
                .ToArray(),
            taskPerformance,
            DateTimeOffset.UtcNow);
    }
}
