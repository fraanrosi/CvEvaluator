using CvEvaluator.Application.DTOs.Analytics;

namespace CvEvaluator.Application.Interfaces;

public interface IAnalyticsService
{
    Task<AnalyticsOverviewDto> GetOverviewAsync(Guid userId, CancellationToken ct);
    Task<ScoreTimeSeriesDto> GetScoresOverTimeAsync(Guid userId, string period, CancellationToken ct);
    Task<ScoreDistributionDto> GetScoreDistributionAsync(Guid userId, CancellationToken ct);
    Task<List<TopCandidateDto>> GetTopCandidatesAsync(Guid userId, Guid jobPositionId, int limit, CancellationToken ct);
}
