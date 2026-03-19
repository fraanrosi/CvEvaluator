using CvEvaluator.Api.Helpers;
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
    private readonly IConfiguration _config;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(
        ISubscriptionService subscriptionService,
        IPaymentService paymentService,
        IConfiguration config,
        ILogger<SubscriptionsController> logger)
    {
        _subscriptionService = subscriptionService;
        _paymentService = paymentService;
        _config = config;
        _logger = logger;
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
        Request.EnableBuffering();

        if (!await MercadoPagoSignatureValidator.IsValidAsync(Request, _config["MercadoPago:WebhookSecret"], _logger))
        {
            _logger.LogWarning("MercadoPago webhook: invalid signature, ignoring");
            return Ok(); // 200 para evitar retries de MP
        }

        if (topic != null && id != null)
            await _paymentService.HandlePaymentNotificationAsync(topic, id.Value, ct);

        return Ok();
    }
}
