using CvEvaluator.Application.DTOs.Analytics;
using CvEvaluator.Application.Interfaces;
using CvEvaluator.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CvEvaluator.Infrastructure.Persistence.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly CvEvaluatorDbContext _db;

    public AnalyticsRepository(CvEvaluatorDbContext db)
    {
        _db = db;
    }

    public async Task<int> CountEvaluationsByUserAsync(Guid userId, CancellationToken ct)
    {
        return await _db.Evaluations
            .CountAsync(e => e.UserId == userId && e.Status == EvaluationStatus.Completed, ct);
    }

    public async Task<decimal> AverageScoreByUserAsync(Guid userId, CancellationToken ct)
    {
        var scores = _db.Evaluations
            .Where(e => e.UserId == userId && e.Status == EvaluationStatus.Completed && e.OverallScore != null);

        if (!await scores.AnyAsync(ct))
            return 0;

        return (decimal)await scores.AverageAsync(e => (double)e.OverallScore!, ct);
    }

    public async Task<int> CountJobPositionsByUserAsync(Guid userId, CancellationToken ct)
    {
        return await _db.JobPositions.CountAsync(j => j.UserId == userId, ct);
    }

    public async Task<int> CountEvaluationsInPeriodAsync(Guid userId, DateTime from, DateTime to, CancellationToken ct)
    {
        return await _db.Evaluations
            .CountAsync(e => e.UserId == userId && e.Status == EvaluationStatus.Completed
                         && e.CreatedAt >= from && e.CreatedAt <= to, ct);
    }

    public async Task<decimal> AverageScoreInPeriodAsync(Guid userId, DateTime from, DateTime to, CancellationToken ct)
    {
        var scores = _db.Evaluations
            .Where(e => e.UserId == userId && e.Status == EvaluationStatus.Completed
                     && e.OverallScore != null && e.CreatedAt >= from && e.CreatedAt <= to);

        if (!await scores.AnyAsync(ct))
            return 0;

        return (decimal)await scores.AverageAsync(e => (double)e.OverallScore!, ct);
    }

    public async Task<string?> GetTopJobPositionTitleAsync(Guid userId, CancellationToken ct)
    {
        return await _db.JobPositions
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => _db.Evaluations.Count(e => e.JobPositionId == j.Id))
            .Select(j => j.Title)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<ScoreDataPoint>> GetScoresOverTimeAsync(Guid userId, DateTime from, DateTime to, CancellationToken ct)
    {
        return await _db.Evaluations
            .Where(e => e.UserId == userId && e.Status == EvaluationStatus.Completed
                     && e.OverallScore != null && e.CreatedAt >= from && e.CreatedAt <= to)
            .GroupBy(e => e.CreatedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new ScoreDataPoint
            {
                Date = g.Key,
                AverageScore = Math.Round((decimal)g.Average(e => (double)e.OverallScore!), 1),
                Count = g.Count()
            })
            .ToListAsync(ct);
    }

    public async Task<List<ScoreBucket>> GetScoreDistributionAsync(Guid userId, CancellationToken ct)
    {
        var ranges = new[] { ("0-20", 0m, 20m), ("21-40", 21m, 40m), ("41-60", 41m, 60m), ("61-80", 61m, 80m), ("81-100", 81m, 100m) };

        var scores = await _db.Evaluations
            .Where(e => e.UserId == userId && e.Status == EvaluationStatus.Completed && e.OverallScore != null)
            .Select(e => e.OverallScore!.Value)
            .ToListAsync(ct);

        return ranges.Select(r => new ScoreBucket
        {
            Range = r.Item1,
            Count = scores.Count(s => s >= r.Item2 && s <= r.Item3)
        }).ToList();
    }

    public async Task<List<TopCandidateDto>> GetTopCandidatesAsync(Guid userId, Guid jobPositionId, int limit, CancellationToken ct)
    {
        return await _db.Evaluations
            .Where(e => e.UserId == userId && e.JobPositionId == jobPositionId
                     && e.Status == EvaluationStatus.Completed && e.OverallScore != null)
            .OrderByDescending(e => e.OverallScore)
            .Take(limit)
            .Select(e => new TopCandidateDto
            {
                EvaluationId = e.Id,
                Filename = e.OriginalFilename,
                OverallScore = e.OverallScore!.Value,
                TechnicalScore = e.TechnicalScore,
                ExperienceScore = e.ExperienceScore,
                EvaluatedAt = e.EvaluatedAt ?? e.CreatedAt
            })
            .ToListAsync(ct);
    }
}
