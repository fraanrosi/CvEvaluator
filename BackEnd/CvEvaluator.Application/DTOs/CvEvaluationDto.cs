using CvEvaluator.Domain.Enums;

namespace CvEvaluator.Application.DTOs;

public class CvEvaluationDto
{
    public Guid Id { get; set; }
    public string OriginalFilename { get; set; }
    public EvaluationStatus Status { get; set; }
    public decimal? OverallScore { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EvaluatedAt { get; set; }
}