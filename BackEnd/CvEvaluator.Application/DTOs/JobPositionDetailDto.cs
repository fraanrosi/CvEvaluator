namespace CvEvaluator.Application.DTOs;

public class JobPositionDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public List<EvaluationSummaryDto> Evaluations { get; set; } = new();
}

public class EvaluationSummaryDto
{
    public Guid Id { get; set; }
    public string CandidateName { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public decimal? Score { get; set; }
    public DateTime? EvaluatedAt { get; set; }
}
