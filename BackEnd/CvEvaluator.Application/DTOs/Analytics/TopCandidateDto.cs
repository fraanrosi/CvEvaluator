namespace CvEvaluator.Application.DTOs.Analytics;

public class TopCandidateDto
{
    public Guid EvaluationId { get; set; }
    public string Filename { get; set; } = string.Empty;
    public decimal OverallScore { get; set; }
    public decimal? TechnicalScore { get; set; }
    public decimal? ExperienceScore { get; set; }
    public DateTime EvaluatedAt { get; set; }
}
