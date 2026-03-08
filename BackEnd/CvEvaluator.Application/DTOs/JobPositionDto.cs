namespace CvEvaluator.Application.DTOs;

public class JobPositionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public int EvaluationsCount { get; set; }
}
