namespace CvEvaluator.Application.DTOs.Analytics;

public class AnalyticsOverviewDto
{
    public int TotalEvaluations { get; set; }
    public decimal AverageScore { get; set; }
    public int TotalJobPositions { get; set; }
    public int EvaluationsThisMonth { get; set; }
    public decimal? ScoreTrendPercent { get; set; }
    public string? TopJobPositionTitle { get; set; }
}
