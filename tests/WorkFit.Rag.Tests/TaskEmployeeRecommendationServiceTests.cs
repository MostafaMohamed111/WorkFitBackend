using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WorkFit.Engine.Contracts.AI;
using WorkFit.Rag.Contracts.Indexing;
using WorkFit.Rag.Contracts.Recommendations;
using WorkFit.Rag.CrossCutting;
using WorkFit.Rag.Features.RecommendEmployees;
using WorkFit.Rag.Infrastructure.Indexing;
using WorkFit.Rag.Infrastructure.Options;
using WorkFit.Rag.Infrastructure.Qdrant;
using WorkFit.TalentManagement.Contracts.Indexing;

namespace WorkFit.Rag.Tests;

public class TaskEmployeeRecommendationServiceTests
{
    private static TaskRecommendationContext Context(Action<TaskRecommendationContextBuilder>? configure = null)
    {
        var builder = new TaskRecommendationContextBuilder();
        configure?.Invoke(builder);
        return builder.Build();
    }

    private static readonly QdrantOptions DefaultQdrant = new();
    private static readonly RagRecommendationOptions DefaultRecommendation = new();

    [Fact]
    public async Task RecommendAsync_NullContext_Throws()
    {
        var harness = new RecommendationHarness();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => harness.Service.RecommendAsync(null!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task RecommendAsync_InvalidResultLimit_Throws(int limit)
    {
        var harness = new RecommendationHarness();
        var context = Context(builder => builder.ResultLimit = limit);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => harness.Service.RecommendAsync(context));
    }

    [Fact]
    public async Task RecommendAsync_NoCandidates_ReturnsEmptyResponse()
    {
        var harness = new RecommendationHarness(employees: Array.Empty<EmployeeIndexingSnapshot>());
        var context = Context();

        var response = await harness.Service.RecommendAsync(context);

        Assert.Empty(response.Employees);
        Assert.Equal(context.TaskId, response.TaskId);
    }

    [Fact]
    public async Task RecommendAsync_SingleCandidate_IsRankedFirstWithGroundedReasoning()
    {
        var employee = Employee("Jane Doe", skills: new[] { Skill("C#", 90) });
        var chatJson = $"{{\"candidates\":[{{\"employeeProfileId\":\"{employee.EmployeeProfileId}\"," +
            "\"efficiencyScore\":0.8,\"explanation\":\"Strong C# match\"}]}";
        var harness = new RecommendationHarness(
            employees: new[] { employee },
            qdrant: new FakeQdrantClient
            {
                EmployeeResults = new[]
                {
                    new QdrantSearchResult(
                        DeterministicPointId.EmployeeProfile(employee.EmployeeProfileId),
                        0.5,
                        PayloadFor(employee))
                }
            },
            chat: new FakeChatClient(chatJson));
        var context = Context(builder => builder.RequiredSkills = new[] { new RequiredSkill(null, "C#", 2, 1) });

        var response = await harness.Service.RecommendAsync(context);

        var result = Assert.Single(response.Employees);
        Assert.Equal(1, result.Rank);
        Assert.Equal(employee.EmployeeProfileId, result.EmployeeProfileId);
        Assert.Contains("C#", result.MatchedSkills);
        Assert.Equal(0.8, result.LlmEfficiencyScore, precision: 10);
        Assert.Equal("Strong C# match", result.GroundedReasoning);
    }

    [Fact]
    public async Task RecommendAsync_LlmFailure_FallsBackToDeterministicScoring()
    {
        var employee = Employee("Jane Doe", skills: new[] { Skill("C#", 90) });
        var harness = new RecommendationHarness(
            employees: new[] { employee },
            qdrant: new FakeQdrantClient
            {
                EmployeeResults = new[]
                {
                    new QdrantSearchResult(
                        DeterministicPointId.EmployeeProfile(employee.EmployeeProfileId),
                        0.5,
                        PayloadFor(employee))
                }
            },
            chat: new FakeChatClient(throwOnSend: true));
        var context = Context(builder => builder.RequiredSkills = new[] { new RequiredSkill(null, "C#", 2, 1) });

        var response = await harness.Service.RecommendAsync(context);

        var result = Assert.Single(response.Employees);
        Assert.Equal(0.4575, result.LlmEfficiencyScore, precision: 10);
        Assert.StartsWith("Based on indexed data:", result.GroundedReasoning);
    }

    [Fact]
    public async Task RecommendAsync_DeduplicatesCandidatesByEmployeeProfileId()
    {
        var employee = Employee("Jane Doe", skills: new[] { Skill("C#", 90) });
        var harness = new RecommendationHarness(
            employees: new[] { employee },
            qdrant: new FakeQdrantClient
            {
                EmployeeResults = new[]
                {
                    new QdrantSearchResult(DeterministicPointId.EmployeeProfile(employee.EmployeeProfileId), 0.5, PayloadFor(employee)),
                    new QdrantSearchResult(DeterministicPointId.EmployeeProfile(employee.EmployeeProfileId), 0.8, PayloadFor(employee))
                }
            });

        var response = await harness.Service.RecommendAsync(
            Context(builder => builder.RequiredSkills = new[] { new RequiredSkill(null, "C#", 2, 1) }));

        Assert.Single(response.Employees);
        Assert.Equal(0.9, response.Employees[0].SemanticScore, precision: 10);
    }

    [Fact]
    public async Task RecommendAsync_RanksCandidatesByFinalScoreDescending()
    {
        var strong = Employee("Strong", skills: new[] { Skill("C#", 95) });
        var weak = Employee("Weak", skills: new[] { Skill("C#", 10) });
        var harness = new RecommendationHarness(
            employees: new[] { strong, weak },
            qdrant: new FakeQdrantClient
            {
                EmployeeResults = new[]
                {
                    new QdrantSearchResult(DeterministicPointId.EmployeeProfile(weak.EmployeeProfileId), -0.5, PayloadFor(weak)),
                    new QdrantSearchResult(DeterministicPointId.EmployeeProfile(strong.EmployeeProfileId), 0.9, PayloadFor(strong))
                }
            },
            chat: new FakeChatClient("""{"candidates":[]}"""));
        var context = Context(builder => builder.RequiredSkills = new[] { new RequiredSkill(null, "C#", 2, 1) });

        var response = await harness.Service.RecommendAsync(context);

        Assert.Equal(2, response.Employees.Count);
        Assert.Equal(strong.EmployeeProfileId, response.Employees[0].EmployeeProfileId);
        Assert.Equal(weak.EmployeeProfileId, response.Employees[1].EmployeeProfileId);
    }

    [Fact]
    public async Task RecommendAsync_HistoricalOutcomesBlendIntoPerformance()
    {
        var employee = Employee("Jane Doe", skills: new[] { Skill("C#", 90) });
        var taskId = Guid.NewGuid();
        var historical = new ProjectTaskIndexDocument(
            TaskId: taskId, ProjectId: Guid.NewGuid(), OrganizationId: employee.OrganizationId,
            TaskTitle: "Old task", TaskDescription: null, TaskType: "Feature", Status: "Done",
            Priority: "Medium", StoryPoints: 20, DueDate: null, ProjectName: "P",
            ProjectDescription: null, ProjectStatus: "Active",
            RequiredSkills: Array.Empty<string>(),
            EmployeeOutcomes: new[]
            {
                new EmployeeTaskOutcomeIndexDocument(employee.EmployeeProfileId, 0.9)
            },
            IsActive: false, Revision: 1, OccurredAt: DateTimeOffset.UtcNow);
        var harness = new RecommendationHarness(
            employees: new[] { employee },
            qdrant: new FakeQdrantClient
            {
                EmployeeResults = new[]
                {
                    new QdrantSearchResult(DeterministicPointId.EmployeeProfile(employee.EmployeeProfileId), 0.5, PayloadFor(employee))
                },
                ProjectTaskResults = new[]
                {
                    new QdrantSearchResult(DeterministicPointId.ProjectTask(taskId), 0.4, JsonElementFrom(historical))
                }
            });
        var context = Context(builder => builder.RequiredSkills = new[] { new RequiredSkill(null, "C#", 2, 1) });

        var response = await harness.Service.RecommendAsync(context);

        var result = Assert.Single(response.Employees);
        Assert.Equal(0.27, result.PerformanceScore, precision: 10);
    }

    private static EmployeeIndexingSnapshot Employee(string name, EmployeeSkillIndexingSnapshot[]? skills = null) => new(
        Guid.NewGuid(), Guid.NewGuid(), name, "Engineer", null, "Active", 0, null,
        skills ?? Array.Empty<EmployeeSkillIndexingSnapshot>(),
        Array.Empty<EmployeeCertificationIndexingSnapshot>(), null, DateTimeOffset.UtcNow);

    private static EmployeeSkillIndexingSnapshot Skill(string name, int confidence) => new(
        Guid.NewGuid(), name, confidence, Array.Empty<EmployeeSkillEvidenceIndexingSnapshot>());

    private static JsonElement PayloadFor(EmployeeIndexingSnapshot employee) => JsonElementFrom(
        EmployeeIndexDocumentMapper.Map(employee));

    private static JsonElement JsonElementFrom<T>(T value) => System.Text.Json.JsonSerializer.SerializeToElement(
        value, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));

    private sealed class RecommendationHarness
    {
        public RecommendationHarness(
            IReadOnlyList<EmployeeIndexingSnapshot>? employees = null,
            FakeQdrantClient? qdrant = null,
            FakeChatClient? chat = null)
        {
            Employees = employees ?? Array.Empty<EmployeeIndexingSnapshot>();
            Qdrant = qdrant ?? new FakeQdrantClient();
            Embedding = new FakeEmbeddingClient();
            Snapshots = new FakeSnapshotService(Employees);
            Chat = chat ?? new FakeChatClient("""{"candidates":[]}""");

            var employeeIndexing = new EmployeeProfileIndexingService(
                Embedding, Qdrant, Options.Create(DefaultQdrant), Options.Create(DefaultRecommendation));

            Service = new TaskEmployeeRecommendationService(
                Embedding,
                Chat,
                Qdrant,
                Snapshots,
                employeeIndexing,
                Options.Create(DefaultQdrant),
                Options.Create(DefaultRecommendation),
                NullLogger<TaskEmployeeRecommendationService>.Instance);
        }

        public TaskEmployeeRecommendationService Service { get; }
        public FakeEmbeddingClient Embedding { get; }
        public FakeQdrantClient Qdrant { get; }
        public FakeChatClient Chat { get; }
        public IReadOnlyList<EmployeeIndexingSnapshot> Employees { get; }
        public FakeSnapshotService Snapshots { get; }
    }

    private sealed class FakeEmbeddingClient : IEmbeddingClient
    {
        public Task<EmbeddingResponse> EmbedAsync(EmbeddingRequest request, CancellationToken cancellationToken = default)
        {
            var vector = new float[QdrantOptions.VectorSize];
            for (var index = 0; index < vector.Length; index++)
            {
                vector[index] = index % 2 == 0 ? 0.5f : -0.5f;
            }

            return Task.FromResult(new EmbeddingResponse(new[] { (ReadOnlyMemory<float>)vector }));
        }
    }

    private sealed class FakeChatClient : IChatCompletionClient
    {
        private readonly string? _content;
        private readonly bool _throw;

        public FakeChatClient(string content) => _content = content;

        public FakeChatClient(bool throwOnSend) => _throw = throwOnSend;

        public Task<ChatCompletionResponse> SendAsync(
            ChatCompletionRequest request,
            CancellationToken cancellationToken = default)
        {
            if (_throw)
            {
                throw new InvalidOperationException("simulated chat failure");
            }

            return Task.FromResult(new ChatCompletionResponse(_content ?? string.Empty, "stop", null, null));
        }
    }

    private sealed class FakeSnapshotService : IEmployeeIndexingSnapshotService
    {
        private readonly IReadOnlyList<EmployeeIndexingSnapshot> _employees;

        public FakeSnapshotService(IReadOnlyList<EmployeeIndexingSnapshot> employees) => _employees = employees;

        public Task<EmployeeIndexingSnapshot?> GetEmployeeAsync(
            Guid employeeProfileId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_employees.FirstOrDefault(employee => employee.EmployeeProfileId == employeeProfileId));
        }

        public Task<IReadOnlyList<EmployeeIndexingSnapshot>> GetOrganizationEmployeesAsync(
            Guid organizationId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<EmployeeIndexingSnapshot>>(
                _employees.Where(employee => employee.OrganizationId == organizationId).ToArray());
        }
    }

    private sealed class FakeQdrantClient : IQdrantRestClient
    {
        public IReadOnlyList<QdrantSearchResult> EmployeeResults { get; init; } =
            Array.Empty<QdrantSearchResult>();
        public IReadOnlyList<QdrantSearchResult> ProjectTaskResults { get; init; } =
            Array.Empty<QdrantSearchResult>();

        public Task EnsureCollectionsAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task UpsertAsync(string collection, QdrantPoint point, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task DeleteAsync(string collection, Guid pointId, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<IReadOnlyList<QdrantSearchResult>> SearchAsync(
            string collection,
            ReadOnlyMemory<float> vector,
            int limit,
            QdrantFilter? filter,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(collection == DefaultQdrant.ProjectTasksCollection
                ? ProjectTaskResults
                : EmployeeResults);
        }
    }
}

internal sealed class TaskRecommendationContextBuilder
{
    public Guid TaskId { get; set; } = Guid.NewGuid();
    public string TaskTitle { get; set; } = "Build a payroll module";
    public string? TaskDescription { get; set; } = "Implement payroll processing.";
    public Guid ProjectId { get; set; } = Guid.NewGuid();
    public string ProjectName { get; set; } = "HCM Suite";
    public string? ProjectDescription { get; set; }
    public Guid OrganizationId { get; set; } = Guid.NewGuid();
    public string? OrganizationName { get; set; } = "Acme";
    public string? Prompt { get; set; }
    public double RequestedAllocation { get; set; } = 20;
    public IReadOnlyList<RequiredSkill> RequiredSkills { get; set; } = new[] { new RequiredSkill(null, "C#", 2, 1) };
    public int ResultLimit { get; set; } = 10;
    public string RequiredEmployeeStatus { get; set; } = "Active";

    public TaskRecommendationContext Build() => new(
        TaskId, TaskTitle, TaskDescription, ProjectId, ProjectName, ProjectDescription,
        OrganizationId, OrganizationName, Prompt, RequestedAllocation, RequiredSkills,
        ResultLimit, RequiredEmployeeStatus);
}
