using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Documents.Features.Queries.GetDocumentById;

public sealed record GetDocumentByIdQuery(
        Guid Id
    ) : IRequest<DocumentStreamResult>;
