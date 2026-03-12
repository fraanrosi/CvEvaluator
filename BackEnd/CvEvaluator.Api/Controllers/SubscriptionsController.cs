using CvEvaluator.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CvEvaluator.Api.Controllers;

[ApiController]
[Route("api/subscriptions")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlans(CancellationToken ct)
    {
        var plans = await _subscriptionService.GetAllPlansAsync(ct);
        return Ok(plans);
    }

    [HttpGet("my-subscription")]
    public async Task<IActionResult> GetMySubscription(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var subscription = await _subscriptionService.GetUserSubscriptionAsync(userId, ct);
        return Ok(subscription);
    }
}
