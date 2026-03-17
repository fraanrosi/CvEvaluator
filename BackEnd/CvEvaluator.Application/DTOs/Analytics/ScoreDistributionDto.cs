namespace CvEvaluator.Application.DTOs.Analytics;

public class ScoreDistributionDto
{
    public List<ScoreBucket> Buckets { get; set; } = new();
}

public class ScoreBucket
{
    public string Range { get; set; } = string.Empty;
    public int Count { get; set; }
}
