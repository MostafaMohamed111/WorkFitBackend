
namespace WorkFit.Documents.Features.Commands.Create.TemporaryUpload;

public sealed record DocumentCreationResponse(
        Guid Id,
        string Name,
        string Type
    );