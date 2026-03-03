using CvEvaluator.Domain.Enums;

namespace CvEvaluator.Domain.Entities;

public class CvEvaluation
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    // Metadata archivo
    public string OriginalFilename { get; set; }
    public long FileSizeBytes { get; set; }
    public string FileHash { get; set; }

    // Contenido procesado
    public string ExtractedText { get; set; }

    // JSONB
    public string? EvaluationResult { get; set; }

    // Scores desnormalizados
    public decimal? OverallScore { get; set; }
    public decimal? TechnicalScore { get; set; }
    public decimal? ExperienceScore { get; set; }
    public decimal? EducationScore { get; set; }

    // Matching opcional
    public string? JobTitle { get; set; }
    public string? JobDescription { get; set; }
    public decimal? MatchScore { get; set; }

    // Metadata proceso
    public string? ModelUsed { get; set; }
    public int? ProcessingTimeMs { get; set; }

    // 🔥 Async state
    public EvaluationStatus Status { get; set; } = EvaluationStatus.Processing;
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EvaluatedAt { get; set; }
}

