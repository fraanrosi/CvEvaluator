using CvEvaluator.Application.DTOs.Analytics;

namespace CvEvaluator.Application.Interfaces;

public interface IAnalyticsRepository
{
    Task<int> CountEvaluationsByUserAsync(Guid userId, CancellationToken ct);
    Task<decimal> AverageScoreByUserAsync(Guid userId, CancellationToken ct);
    Task<int> CountJobPositionsByUserAsync(Guid userId, CancellationToken ct);
    Task<int> CountEvaluationsInPeriodAsync(Guid userId, DateTime from, DateTime to, CancellationToken ct);
    Task<decimal> AverageScoreInPeriodAsync(Guid userId, DateTime from, DateTime to, CancellationToken ct);
    Task<string?> GetTopJobPositionTitleAsync(Guid userId, CancellationToken ct);
    Task<List<ScoreDataPoint>> GetScoresOverTimeAsync(Guid userId, DateTime from, DateTime to, CancellationToken ct);
    Task<List<ScoreBucket>> GetScoreDistributionAsync(Guid userId, CancellationToken ct);
    Task<List<TopCandidateDto>> GetTopCandidatesAsync(Guid userId, Guid jobPositionId, int limit, CancellationToken ct);
}
