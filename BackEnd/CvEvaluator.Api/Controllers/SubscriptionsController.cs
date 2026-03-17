using CvEvaluator.Application.DTOs;
using CvEvaluator.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace CvEvaluator.Api.Controllers;

[ApiController]
[Route("api/subscriptions")]
[Authorize]
[EnableRateLimiting("general")]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IPaymentService _paymentService;

    public SubscriptionsController(
        ISubscriptionService subscriptionService,
        IPaymentService paymentService)
    {
        _subscriptionService = subscriptionService;
        _paymentService = paymentService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

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
        var subscription = await _subscriptionService.GetUserSubscriptionAsync(GetUserId(), ct);
        return Ok(subscription);
    }

    [HttpPost("create-checkout")]
    public async Task<IActionResult> CreateCheckout([FromBody] CreateCheckoutRequestDto request, CancellationToken ct)
    {
        var userId = GetUserId();
        var email = User.FindFirstValue(ClaimTypes.Email) ?? "";
        var initPoint = await _paymentService.CreateCheckoutAsync(userId, email, request.PlanId, ct);
        return Ok(new { initPoint });
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> MercadoPagoWebhook(
        [FromQuery] string? topic,
        [FromQuery] long? id,
        CancellationToken ct)
    {
        if (topic != null && id != null)
        {
            await _paymentService.HandlePaymentNotificationAsync(topic, id.Value, ct);
        }
        return Ok();
    }
}
