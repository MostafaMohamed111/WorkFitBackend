using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Contracts.CreateProjectService;
using WorkFit.ProjectManagement.Contracts.CreateProjectTaskService;
using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Infrastructure;

namespace WorkFit.ProjectManagement.CrossCutting;

internal sealed class CreateProjectService : ICreateProjectService
{
    private readonly WorkFitProjectDbContext _dbContext;

    public CreateProjectService(WorkFitProjectDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> UpsertExternalProjectAsync(UpsertExternalProjectDto dto, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<SourceSystem>(dto.SourceSystem, true, out var sourceSystem))
        {
            sourceSystem = SourceSystem.Internal;
        }

        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(p => p.SourceSystem == sourceSystem && p.SourceReferenceId == dto.SourceReferenceId, cancellationToken);

        if (project is null)
        {
            var status = ProjectStatus.Active;
            project = Project.Create(
                organizationId: dto.OrganizationId,
                name: dto.Name,
                projectDocumentIds: [],
                description: dto.Description,
                startDate: null,
                endDate: null,
                teamLeaderId: null, // external projects might not have team leader initially
                status: status,
                sourceSystem: sourceSystem,
                sourceReferenceId: dto.SourceReferenceId
            );

            _dbContext.Projects.Add(project);
        }
        else
        {
            project.UpdateDetails(dto.Name, dto.Description, project.EndDate);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return project.Id;
    }
}
