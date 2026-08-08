using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WorkFit.Engine.Contracts.AI;
using WorkFit.Engine.Infrastructure.AI;
using WorkFit.Engine.Infrastructure.CVParsing;
using WorkFit.Engine.Infrastructure.Extraction;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath("d:/ITI Graduation/WorkFitBackend/src/WorkFit.Host")
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var services = new ServiceCollection();
services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Information));
services.AddSingleton<IConfiguration>(config);
services.AddOptions();
services.Configure<AIOptions>(config.GetSection("AI"));
services.AddHttpClient("EngineMistral", c => c.Timeout = TimeSpan.FromSeconds(120));
services.AddSingleton<IChatCompletionClient, MistralChatCompletionClient>();
services.AddSingleton<ICVLLMParser, CVLLMParser>();
services.AddSingleton<ICVTextExtractor, PdfTextExtractor>();
services.AddSingleton<CVTextExtractorAggregator>();

var provider = services.BuildServiceProvider();
var extractor = provider.GetRequiredService<CVTextExtractorAggregator>();
var parser = provider.GetRequiredService<ICVLLMParser>();

var pdfPath = "d:/ITI Graduation/WorkFitBackend/Karim Ayman Elmliegy.pdf";
await using var fs = File.OpenRead(pdfPath);
var result = await extractor.ExtractAsync(Path.GetFileName(pdfPath), "application/pdf", fs);
Console.WriteLine($"Extraction success={result.Success} source={result.Source}");
Console.WriteLine($"Extracted text length={result.Text?.Length ?? 0}");
if (!result.Success || string.IsNullOrWhiteSpace(result.Text))
    return;

var parsed = await parser.ParseAsync(result.Text);
Console.WriteLine(JsonSerializer.Serialize(parsed, new JsonSerializerOptions { WriteIndented = true }));
