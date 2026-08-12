namespace WorkFit.Rag.Contracts.Indexing;

public interface IEmployeeProfileIndexingService
{
    Task UpsertAsync(EmployeeProfileIndexDocument document, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid employeeProfileId, CancellationToken cancellationToken = default);
}
