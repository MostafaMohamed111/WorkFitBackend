using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Integration.Features.Commands.DeleteJiraSettings;

internal sealed record DeleteJiraSettingsCommand(Guid OrganizationId) : IRequest;
