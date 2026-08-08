namespace WorkFit.Engine.Contracts.CVParsing;

public sealed record ParsedCV(
    bool IsCV,
    string? Name,
    string? Email,
    string? Phone,
    string? JobTitle,
    string? Summary,
    string? LinkedInUrl,
    IReadOnlyList<ParsedSkill> Skills,
    IReadOnlyList<ParsedExperience> Experiences,
    IReadOnlyList<ParsedEducation> Education,
    IReadOnlyList<ParsedCertification> Certifications,
    IReadOnlyList<ParsedLanguage> Languages,
    int LLMConfidence);
