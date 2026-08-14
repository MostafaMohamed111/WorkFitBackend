using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.Engine.Contracts.AI;
using WorkFit.Engine.Contracts.CVParsing;
using WorkFit.Engine.Infrastructure.AI;
using WorkFit.Engine.Infrastructure.CVParsing;
using WorkFit.Engine.Infrastructure.Data;
using WorkFit.Engine.Infrastructure.Extraction;
using WorkFit.SharedKernel.DependencyInjection;
using WorkFit.SharedKernel.RegisterModuleServices;

namespace WorkFit.Engine;

public sealed class RegisterEngineModuleServices : IRegisterModuleServices
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.Configure<AIOptions>(configuration.GetSection("AI"));

        services.AddDbContext<EngineDbContext>(options => options.UseSqlServer(connectionString));

        services.AddHttpClient("EngineMistral", c => c.Timeout = TimeSpan.FromSeconds(120));
        services.AddHttpClient("EngineMistralEmbedding", c => c.Timeout = TimeSpan.FromSeconds(60));

        services.AddScoped<ICVTextExtractor, PdfTextExtractor>();
        services.AddScoped<ICVTextExtractor, DocxTextExtractor>();
        services.AddScoped<CVTextExtractorAggregator>();
        services.AddScoped<ICVLLMParser, CVLLMParser>();
        services.AddScoped<ICVSkillNormalizer, CVSkillNormalizer>();
        services.AddScoped<IParseCVDocumentsService, ParseCVDocumentsService>();

        services.AddSingleton<IChatCompletionClient, MistralChatCompletionClient>();
        services.AddSingleton<IEmbeddingClient, MistralEmbeddingClient>();

        services.AddMediatorHandlers<ModuleMarker>();
    }
}
