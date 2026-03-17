using CvEvaluator.Application.DTOs.Analytics;
using CvEvaluator.Application.Interfaces;
using CvEvaluator.Application.Services;
using Moq;

namespace CvEvaluator.Application.Tests;

public class AnalyticsServiceTests
{
    private readonly Mock<IAnalyticsRepository> _repoMock;
    private readonly AnalyticsService _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public AnalyticsServiceTests()
    {
        _repoMock = new Mock<IAnalyticsRepository>();
        _sut = new AnalyticsService(_repoMock.Object);
    }

    // ─── GetOverviewAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetOverviewAsync_ReturnsCorrectCounts()
    {
        _repoMock.Setup(r => r.CountEvaluationsByUserAsync(_userId, default)).ReturnsAsync(10);
        _repoMock.Setup(r => r.AverageScoreByUserAsync(_userId, default)).ReturnsAsync(75.5m);
        _repoMock.Setup(r => r.CountJobPositionsByUserAsync(_userId, default)).ReturnsAsync(3);
        _repoMock.Setup(r => r.CountEvaluationsInPeriodAsync(_userId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), default)).ReturnsAsync(4);
        _repoMock.Setup(r => r.GetTopJobPositionTitleAsync(_userId, default)).ReturnsAsync("Backend Developer");
        _repoMock.Setup(r => r.AverageScoreInPeriodAsync(_userId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), default)).ReturnsAsync(0m);

        var result = await _sut.GetOverviewAsync(_userId, default);

        Assert.Equal(10, result.TotalEvaluations);
        Assert.Equal(75.5m, result.AverageScore);
        Assert.Equal(3, result.TotalJobPositions);
        Assert.Equal(4, result.EvaluationsThisMonth);
        Assert.Equal("Backend Developer", result.TopJobPositionTitle);
    }

    [Fact]
    public async Task GetOverviewAsync_WhenNoEvaluations_TrendIsNull()
    {
        _repoMock.Setup(r => r.CountEvaluationsByUserAsync(_userId, default)).ReturnsAsync(0);
        _repoMock.Setup(r => r.AverageScoreByUserAsync(_userId, default)).ReturnsAsync(0m);
        _repoMock.Setup(r => r.CountJobPositionsByUserAsync(_userId, default)).ReturnsAsync(0);
        _repoMock.Setup(r => r.CountEvaluationsInPeriodAsync(_userId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), default)).ReturnsAsync(0);
        _repoMock.Setup(r => r.GetTopJobPositionTitleAsync(_userId, default)).ReturnsAsync((string?)null);

        var result = await _sut.GetOverviewAsync(_userId, default);

        Assert.Null(result.ScoreTrendPercent);
        _repoMock.Verify(r => r.AverageScoreInPeriodAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetOverviewAsync_CalculatesTrendPercentCorrectly()
    {
        _repoMock.Setup(r => r.CountEvaluationsByUserAsync(_userId, default)).ReturnsAsync(5);
        _repoMock.Setup(r => r.AverageScoreByUserAsync(_userId, default)).ReturnsAsync(80m);
        _repoMock.Setup(r => r.CountJobPositionsByUserAsync(_userId, default)).ReturnsAsync(1);
        _repoMock.Setup(r => r.CountEvaluationsInPeriodAsync(_userId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), default)).ReturnsAsync(2);
        _repoMock.Setup(r => r.GetTopJobPositionTitleAsync(_userId, default)).ReturnsAsync("Dev");

        // current month avg=88, prev month avg=80 → trend = +10%
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfPrevMonth = startOfMonth.AddMonths(-1);

        _repoMock.Setup(r => r.AverageScoreInPeriodAsync(_userId, It.Is<DateTime>(d => d >= startOfMonth), It.IsAny<DateTime>(), default))
                 .ReturnsAsync(88m);
        _repoMock.Setup(r => r.AverageScoreInPeriodAsync(_userId, It.Is<DateTime>(d => d < startOfMonth), It.IsAny<DateTime>(), default))
                 .ReturnsAsync(80m);

        var result = await _sut.GetOverviewAsync(_userId, default);

        Assert.Equal(10m, result.ScoreTrendPercent);
    }

    // ─── GetScoresOverTimeAsync ────────────────────────────────────────

    [Theory]
    [InlineData("7d", -7)]
    [InlineData("30d", -30)]
    [InlineData("90d", -90)]
    [InlineData("invalid", -30)]
    public async Task GetScoresOverTimeAsync_ParsesPeriodCorrectly(string period, int expectedDays)
    {
        var points = new List<ScoreDataPoint> { new() { Date = DateTime.UtcNow, AverageScore = 70m, Count = 1 } };

        DateTime capturedFrom = default;
        _repoMock.Setup(r => r.GetScoresOverTimeAsync(_userId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), default))
                 .Callback<Guid, DateTime, DateTime, CancellationToken>((_, from, _, _) => capturedFrom = from)
                 .ReturnsAsync(points);

        var result = await _sut.GetScoresOverTimeAsync(_userId, period, default);

        var expectedFrom = DateTime.UtcNow.AddDays(expectedDays);
        Assert.InRange(capturedFrom, expectedFrom.AddSeconds(-5), expectedFrom.AddSeconds(5));
        Assert.Single(result.DataPoints);
    }

    // ─── GetScoreDistributionAsync ─────────────────────────────────────

    [Fact]
    public async Task GetScoreDistributionAsync_ReturnsBuckets()
    {
        var buckets = new List<ScoreBucket>
        {
            new() { Range = "0-20", Count = 1 },
            new() { Range = "80-100", Count = 5 }
        };
        _repoMock.Setup(r => r.GetScoreDistributionAsync(_userId, default)).ReturnsAsync(buckets);

        var result = await _sut.GetScoreDistributionAsync(_userId, default);

        Assert.Equal(2, result.Buckets.Count);
        Assert.Equal("80-100", result.Buckets[1].Range);
        Assert.Equal(5, result.Buckets[1].Count);
    }

    // ─── GetTopCandidatesAsync ─────────────────────────────────────────

    [Fact]
    public async Task GetTopCandidatesAsync_DelegatesToRepository()
    {
        var jobPositionId = Guid.NewGuid();
        var candidates = new List<TopCandidateDto>
        {
            new() { Filename = "cv_alice.pdf", OverallScore = 92m },
            new() { Filename = "cv_bob.pdf", OverallScore = 85m }
        };
        _repoMock.Setup(r => r.GetTopCandidatesAsync(_userId, jobPositionId, 5, default)).ReturnsAsync(candidates);

        var result = await _sut.GetTopCandidatesAsync(_userId, jobPositionId, 5, default);

        Assert.Equal(2, result.Count);
        Assert.Equal("cv_alice.pdf", result[0].Filename);
        _repoMock.Verify(r => r.GetTopCandidatesAsync(_userId, jobPositionId, 5, default), Times.Once);
    }
}
