using CvEvaluator.Domain.Enums;

namespace CvEvaluator.Application.DTOs;

public class CvEvaluationDto
{
    public Guid Id { get; set; }
    public Guid JobPositionId { get; set; }
    public string OriginalFilename { get; set; }
    public EvaluationStatus Status { get; set; }
    public decimal? OverallScore { get; set; }
    public decimal? TechnicalScore { get; set; }
    public decimal? ExperienceScore { get; set; }
    public decimal? EducationScore { get; set; }
    public List<string>? Strengths { get; set; }
    public List<string>? Weaknesses { get; set; }
    public int? YearsExperience { get; set; }
    public bool? MatchesRequirements { get; set; }
    public string? ModelUsed { get; set; }
    public int? ProcessingTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EvaluatedAt { get; set; }
}