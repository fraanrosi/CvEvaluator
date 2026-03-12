namespace CvEvaluator.Application.DTOs;

public class PlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MaxEvaluationsPerMonth { get; set; }
    public int MaxJobPositions { get; set; }
}
