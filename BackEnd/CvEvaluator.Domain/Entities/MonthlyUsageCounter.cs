namespace CvEvaluator.Domain.Entities;

public class MonthlyUsageCounter
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int EvaluationCount { get; set; }
}
