namespace CvEvaluator.Application.DTOs.Analytics;

public class ScoreTimeSeriesDto
{
    public List<ScoreDataPoint> DataPoints { get; set; } = new();
}

public class ScoreDataPoint
{
    public DateTime Date { get; set; }
    public decimal AverageScore { get; set; }
    public int Count { get; set; }
}
