using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.Engine.Contracts.AI;
using WorkFit.Engine.Infrastructure.AI;
using WorkFit.Engine.Infrastructure.CVParsing;
using WorkFit.Engine.Infrastructure.Data;
using WorkFit.Engine.Infrastructure.Extraction;
using WorkFit.Engine.Infrastructure.Options;
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
        services.Configure<CVParsingOptions>(configuration.GetSection("CVParsing"));

        services.AddDbContext<EngineDbContext>(options => options.UseSqlServer(connectionString));

        services.AddHttpClient("EngineMistral", c => c.Timeout = TimeSpan.FromSeconds(120));
        services.AddHttpClient("EngineMistralEmbedding", c => c.Timeout = TimeSpan.FromSeconds(60));
        services.AddHttpClient("EngineGeminiEmbedding", c =>
        {
            c.BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/");
            c.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddSingleton<CVProcessingChannel>();
        services.AddHostedService<BackgroundCVWorker>();

        services.AddScoped<ICVTextExtractor, PdfTextExtractor>();
        services.AddScoped<ICVTextExtractor, DocxTextExtractor>();
        services.AddScoped<CVTextExtractorAggregator>();
        services.AddScoped<ICVLLMParser, CVLLMParser>();
        services.AddScoped<ICVSkillNormalizer, CVSkillNormalizer>();
        services.AddScoped<ICVParsePipeline, CVParsePipeline>();

        services.AddSingleton<IChatCompletionClient, MistralChatCompletionClient>();
        services.AddSingleton<IEmbeddingClient, GeminiEmbeddingClient>();

        services.AddMediatorHandlers<ModuleMarker>();
    }
}
