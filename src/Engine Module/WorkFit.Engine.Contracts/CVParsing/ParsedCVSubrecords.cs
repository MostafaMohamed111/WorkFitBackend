namespace WorkFit.Engine.Contracts.CVParsing;

public sealed record ParsedSkill(
    string Name,
    int? YearsExperience,
    int ConfidenceScore,
    string? Evidence);

public sealed record ParsedExperience(
    string? Company,
    string? Role,
    string? StartDate,
    string? EndDate,
    string? Summary);

public sealed record ParsedEducation(
    string? Institution,
    string? Degree,
    string? StartDate,
    string? EndDate);

public sealed record ParsedCertification(
    string Name,
    string? Issuer,
    string? IssueDate,
    string? ExpiryDate);

public sealed record ParsedLanguage(
    string Name,
    string? Level);
