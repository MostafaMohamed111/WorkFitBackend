using System.Text.Json;
using WorkFit.Recommendations.Domain.Entities;
using WorkFit.Recommendations.Domain.Services;
using WorkFit.Recommendations.Infrastructure.Data;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Contracts.LookUpServices;

namespace WorkFit.Recommendations.Features.GenerateRecommendation;

public sealed class GenerateRecommendationCommandHandler
    : IRequestHandler<GenerateRecommendationCommand, GenerateRecommendationResponse>
{
    private readonly RecommendationDbContext _context;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IEmployeeLookUpService _employeeLookUpService;
    private readonly IRecommendationScoringService _scoringService;

    public GenerateRecommendationCommandHandler(
        RecommendationDbContext context,
        ICurrentUserContext currentUserContext,
        IEmployeeLookUpService employeeLookUpService,
        IRecommendationScoringService scoringService)
    {
        _context = context;
        _currentUserContext = currentUserContext;
        _employeeLookUpService = employeeLookUpService;
        _scoringService = scoringService;
    }

    public async Task<GenerateRecommendationResponse> Handle(
        GenerateRecommendationCommand command,
        CancellationToken ct)
    {
        var allEmployees = await _employeeLookUpService.GetAllEmployeesAsync(ct);

        var candidateInputs = new List<(Guid EmployeeId, decimal MatchScore, string MatchReasoning, string AdditionalSkills)>();

        foreach (var employee in allEmployees.Where(e => e.IsActive))
        {
            var (score, reasoning) = _scoringService.CalculateScore(employee.Skills, command.RequiredSkillIds);
            
            if (score > 0)
            {
                var additionalSkills = employee.Skills
                    .Where(s => !command.RequiredSkillIds.Contains(s.SkillId))
                    .OrderByDescending(s => s.ConfidenceScore)
                    .Take(5)
                    .Select(s => new AdditionalSkillDto(s.SkillId, s.SkillName, s.ConfidenceScore))
                    .ToList();
                
                var additionalSkillsJson = JsonSerializer.Serialize(additionalSkills);

                candidateInputs.Add((employee.Id, score, reasoning, additionalSkillsJson));
            }
        }

        var userId = _currentUserContext.GetUserId(ct);
        var recommendation = Recommendation.Create(
            command.TaskId,
            userId,
            command.RequiredSkillIds,
            candidateInputs);

        _context.Recommendations.Add(recommendation);

        await _context.SaveChangesAsync(ct);

        var responseCandidates = recommendation.Candidates
            .OrderBy(c => c.Rank)
            .Select(c => new GenerateRecommendationCandidateDto(
                c.EmployeeId, c.MatchScore, c.MatchReasoning, c.Rank, 
                c.Status, c.ReviewedAt,
                JsonSerializer.Deserialize<List<AdditionalSkillDto>>(c.AdditionalSkills) ?? new()))
            .ToList();

        return new GenerateRecommendationResponse(
            recommendation.Id,
            recommendation.TaskId,
            recommendation.CreatedBy,
            recommendation.CreatedAt,
            recommendation.Candidates.Count,
            responseCandidates);
    }
}
