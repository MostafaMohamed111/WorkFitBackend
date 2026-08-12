using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.Rag.Contracts.Indexing;
using WorkFit.Rag.Contracts.Recommendations;
using WorkFit.Rag.Features.RecommendEmployees;
using WorkFit.Rag.Infrastructure.Indexing;
using WorkFit.Rag.Infrastructure.Options;
using WorkFit.Rag.Infrastructure.Qdrant;
using WorkFit.SharedKernel.DependencyInjection;
using WorkFit.SharedKernel.RegisterModuleServices;

namespace WorkFit.Rag.Infrastructure;

public sealed class RegisterRagModuleServices : IRegisterModuleServices
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var qdrantSection = configuration.GetSection(QdrantOptions.SectionName);
        services.Configure<QdrantOptions>(qdrantSection);
        services.Configure<RagRecommendationOptions>(
            configuration.GetSection(RagRecommendationOptions.SectionName));

        var qdrant = qdrantSection.Get<QdrantOptions>() ?? new QdrantOptions();
        if (!Uri.TryCreate(qdrant.Url, UriKind.Absolute, out var qdrantUri))
        {
            throw new InvalidOperationException($"Configuration '{QdrantOptions.SectionName}:Url' must be an absolute URL.");
        }

        if (string.IsNullOrWhiteSpace(qdrant.EmployeeProfilesCollection) ||
            string.IsNullOrWhiteSpace(qdrant.ProjectTasksCollection))
        {
            throw new InvalidOperationException("RAG Qdrant collection names cannot be empty.");
        }

        services.AddHttpClient("RagQdrant", client =>
        {
            client.BaseAddress = new Uri(qdrantUri.ToString().TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(Math.Clamp(qdrant.TimeoutSeconds, 1, 300));
            if (!string.IsNullOrWhiteSpace(qdrant.ApiKey))
            {
                client.DefaultRequestHeaders.Add("api-key", qdrant.ApiKey);
            }

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        services.AddSingleton<IQdrantRestClient, QdrantRestClient>();
        services.AddScoped<IEmployeeProfileIndexingService, EmployeeProfileIndexingService>();
        services.AddScoped<IProjectTaskIndexingService, ProjectTaskIndexingService>();
        services.AddScoped<ITaskEmployeeRecommendationService, TaskEmployeeRecommendationService>();
        services.AddMediatorHandlers<ModuleMarker>();
    }
}
