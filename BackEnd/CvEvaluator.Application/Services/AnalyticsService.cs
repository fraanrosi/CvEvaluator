using CvEvaluator.Application.DTOs.Analytics;
using CvEvaluator.Application.Interfaces;

namespace CvEvaluator.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _repo;

    public AnalyticsService(IAnalyticsRepository repo)
    {
        _repo = repo;
    }

    public async Task<AnalyticsOverviewDto> GetOverviewAsync(Guid userId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfPrevMonth = startOfMonth.AddMonths(-1);

        var totalEvals = await _repo.CountEvaluationsByUserAsync(userId, ct);
        var avgScore = await _repo.AverageScoreByUserAsync(userId, ct);
        var totalPositions = await _repo.CountJobPositionsByUserAsync(userId, ct);
        var evalsThisMonth = await _repo.CountEvaluationsInPeriodAsync(userId, startOfMonth, now, ct);
        var topTitle = await _repo.GetTopJobPositionTitleAsync(userId, ct);

        decimal? trend = null;
        if (totalEvals > 0)
        {
            var avgCurrent = await _repo.AverageScoreInPeriodAsync(userId, startOfMonth, now, ct);
            var avgPrev = await _repo.AverageScoreInPeriodAsync(userId, startOfPrevMonth, startOfMonth, ct);
            if (avgPrev > 0)
                trend = Math.Round(((avgCurrent - avgPrev) / avgPrev) * 100, 1);
        }

        return new AnalyticsOverviewDto
        {
            TotalEvaluations = totalEvals,
            AverageScore = Math.Round(avgScore, 1),
            TotalJobPositions = totalPositions,
            EvaluationsThisMonth = evalsThisMonth,
            ScoreTrendPercent = trend,
            TopJobPositionTitle = topTitle
        };
    }

    public async Task<ScoreTimeSeriesDto> GetScoresOverTimeAsync(Guid userId, string period, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var from = period switch
        {
            "7d" => now.AddDays(-7),
            "90d" => now.AddDays(-90),
            _ => now.AddDays(-30) // default 30d
        };

        var points = await _repo.GetScoresOverTimeAsync(userId, from, now, ct);
        return new ScoreTimeSeriesDto { DataPoints = points };
    }

    public async Task<ScoreDistributionDto> GetScoreDistributionAsync(Guid userId, CancellationToken ct)
    {
        var buckets = await _repo.GetScoreDistributionAsync(userId, ct);
        return new ScoreDistributionDto { Buckets = buckets };
    }

    public async Task<List<TopCandidateDto>> GetTopCandidatesAsync(Guid userId, Guid jobPositionId, int limit, CancellationToken ct)
    {
        return await _repo.GetTopCandidatesAsync(userId, jobPositionId, limit, ct);
    }
}
