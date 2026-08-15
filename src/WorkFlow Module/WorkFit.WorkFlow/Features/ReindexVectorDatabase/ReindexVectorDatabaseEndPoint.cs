using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Organizations.Contracts.OrganizationServices;
using WorkFit.Rag.Contracts.Indexing;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.TalentManagement.Contracts.Indexing;

namespace WorkFit.WorkFlow.Features.ReindexVectorDatabase;

public sealed record ReindexVectorDatabaseResponse(
    int IndexedEmployees,
    string Message);

public sealed class ReindexVectorDatabaseEndPoint : EndpointWithoutRequest<ReindexVectorDatabaseResponse>
{
    private readonly ICurrentUserContext _currentUser;
    private readonly IGetOrganizationIdService _organizations;
    private readonly IEmployeeIndexingSnapshotService _employeeSnapshots;
    private readonly IEmployeeProfileIndexingService _employeeIndexing;

    public ReindexVectorDatabaseEndPoint(
        ICurrentUserContext currentUser,
        IGetOrganizationIdService organizations,
        IEmployeeIndexingSnapshotService employeeSnapshots,
        IEmployeeProfileIndexingService employeeIndexing)
    {
        _currentUser = currentUser;
        _organizations = organizations;
        _employeeSnapshots = employeeSnapshots;
        _employeeIndexing = employeeIndexing;
    }

    public override void Configure()
    {
        Post("/api/rag/reindex");
        Options(x => x.WithTags("Agent", "Recommendations", "RAG"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        Guid organizationId = Guid.Empty;
        try
        {
            var userId = _currentUser.GetUserId(cancellationToken);
            organizationId = await _organizations.GetOrganizationIdAsync(userId, cancellationToken);
        }
        catch
        {
        }

        // Fetch all employees in SQL database (across organization if Guid.Empty)
        var employees = await _employeeSnapshots.GetOrganizationEmployeesAsync(organizationId, cancellationToken);
        var indexedEmployeesCount = 0;

        foreach (var employee in employees)
        {
            if (employee.EmployeeProfileId == Guid.Empty || employee.OrganizationId == Guid.Empty)
            {
                continue;
            }

            if (string.Equals(employee.Status, "Deleted", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(employee.Status, "Inactive", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var skills = employee.Skills
                .Where(s => !string.IsNullOrWhiteSpace(s.Name))
                .Select(s => new EmployeeSkillIndexDocument(
                    s.SkillId,
                    s.Name,
                    Math.Clamp(s.ConfidenceScore / 100d, 0, 1)))
                .ToList();

            var summary = $"Job title: {employee.JobTitle ?? "Developer"}. Bio: {employee.Bio ?? "WorkFit team member"}. Skills: {string.Join(", ", skills.Select(s => s.Name))}.";

            var doc = new EmployeeProfileIndexDocument(
                employee.EmployeeProfileId,
                employee.OrganizationId,
                employee.Name,
                employee.Status,
                Math.Max(0, 100 - employee.CurrentAllocationPercentage),
                0.8,
                summary,
                skills);

            await _employeeIndexing.UpsertAsync(doc, cancellationToken);
            indexedEmployeesCount++;
        }

        await Send.OkAsync(
            new ReindexVectorDatabaseResponse(
                indexedEmployeesCount,
                $"Successfully generated vector embeddings and indexed {indexedEmployeesCount} employee profiles into Qdrant Vector DB."),
            cancellationToken);
    }
}
