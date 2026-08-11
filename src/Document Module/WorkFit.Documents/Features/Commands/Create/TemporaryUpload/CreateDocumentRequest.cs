using WorkFit.Documents.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace WorkFit.Documents.Features.Commands.Create.TemporaryUpload;

public sealed record CreateDocumentRequest(
        IFormFile File,
        Guid OrganizationId
    );