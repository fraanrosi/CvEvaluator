using CvEvaluator.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace CvEvaluator.Api.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
[EnableRateLimiting("general")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken ct)
    {
        var result = await _analyticsService.GetOverviewAsync(GetUserId(), ct);
        return Ok(result);
    }

    [HttpGet("scores-over-time")]
    public async Task<IActionResult> GetScoresOverTime([FromQuery] string period = "30d", CancellationToken ct = default)
    {
        var result = await _analyticsService.GetScoresOverTimeAsync(GetUserId(), period, ct);
        return Ok(result);
    }

    [HttpGet("score-distribution")]
    public async Task<IActionResult> GetScoreDistribution(CancellationToken ct)
    {
        var result = await _analyticsService.GetScoreDistributionAsync(GetUserId(), ct);
        return Ok(result);
    }

    [HttpGet("top-candidates")]
    public async Task<IActionResult> GetTopCandidates([FromQuery] Guid jobPositionId, [FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var result = await _analyticsService.GetTopCandidatesAsync(GetUserId(), jobPositionId, limit, ct);
        return Ok(result);
    }
}
