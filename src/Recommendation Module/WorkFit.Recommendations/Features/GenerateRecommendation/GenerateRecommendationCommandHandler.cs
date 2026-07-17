using WorkFit.Recommendations.Domain.Entities;
using WorkFit.Recommendations.Domain.Services;
using WorkFit.Recommendations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Contracts.LookUpServices;

namespace WorkFit.Recommendations.Features.GenerateRecommendation;

public sealed class GenerateRecommendationCommandHandler
    : IRequestHandler<GenerateRecommendationCommand, GenerateRecommendationResponse>
{
    private readonly RecommendationDbContext _context;
    private readonly IEmployeeLookUpService _employeeLookUpService;
    private readonly IRecommendationScoringService _scoringService;

    public GenerateRecommendationCommandHandler(
        RecommendationDbContext context,
        IEmployeeLookUpService employeeLookUpService,
        IRecommendationScoringService scoringService)
    {
        _context = context;
        _employeeLookUpService = employeeLookUpService;
        _scoringService = scoringService;
    }

    public async Task<GenerateRecommendationResponse> Handle(
        GenerateRecommendationCommand command,
        CancellationToken ct)
    {
        var allEmployees = await _employeeLookUpService.GetAllEmployeesAsync(ct);

        var candidateInputs = new List<(Guid EmployeeId, decimal MatchScore, string MatchReasoning)>();

        foreach (var employee in allEmployees.Where(e => e.IsActive))
        {
            var (score, reasoning) = _scoringService.CalculateScore(employee.Skills, command.RequiredSkillIds);
            
            // Only consider candidates with a score greater than 0
            if (score > 0)
            {
                candidateInputs.Add((employee.Id, score, reasoning));
            }
        }

        var recommendation = Recommendation.Create(
            command.TaskId,
            command.RequiredSkillIds,
            candidateInputs);

        _context.Recommendations.Add(recommendation);

        await _context.SaveChangesAsync(ct);

        var responseCandidates = recommendation.Candidates
            .OrderBy(c => c.Rank)
            .Select(c => new GenerateRecommendationCandidateDto(
                c.EmployeeId, c.MatchScore, c.MatchReasoning, c.Rank, 
                c.Status, c.ApprovedBy, c.ApprovedAt, c.RejectedBy, c.RejectedAt))
            .ToList();

        return new GenerateRecommendationResponse(
            recommendation.Id,
            recommendation.TaskId,
            recommendation.GeneratedAt,
            recommendation.Candidates.Count,
            responseCandidates);
    }
}
