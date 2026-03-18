using CvEvaluator.Application.Interfaces;
using CvEvaluator.Domain.Enums;
using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Payment;
using MercadoPago.Resource.Preference;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CvEvaluator.Infrastructure.Payments;

public class MercadoPagoPaymentService : IPaymentService
{
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;
    private readonly ILogger<MercadoPagoPaymentService> _logger;

    public MercadoPagoPaymentService(
        ISubscriptionRepository subscriptionRepo,
        IUnitOfWork unitOfWork,
        IConfiguration config,
        ILogger<MercadoPagoPaymentService> logger)
    {
        _subscriptionRepo = subscriptionRepo;
        _unitOfWork = unitOfWork;
        _config = config;
        _logger = logger;
    }

    public async Task<string> CreateCheckoutAsync(Guid userId, string userEmail, Guid planId, CancellationToken ct)
    {
        var plan = await _subscriptionRepo.GetPlanByIdAsync(planId, ct)
            ?? throw new ArgumentException("Plan not found");

        if (plan.Price <= 0)
            throw new ArgumentException("Cannot create checkout for free plan");

        var client = new PreferenceClient();
        var request = new PreferenceRequest
        {
            Items = new List<PreferenceItemRequest>
            {
                new()
                {
                    Title = $"CvEvaluator - Plan {plan.Name}",
                    Quantity = 1,
                    CurrencyId = plan.Currency,
                    UnitPrice = plan.Price,
                }
            },
            Payer = new PreferencePayerRequest { Email = userEmail },
            BackUrls = new PreferenceBackUrlsRequest
            {
                Success = _config["MercadoPago:SuccessUrl"] ?? "http://localhost:4200/subscription/success",
                Failure = _config["MercadoPago:FailureUrl"] ?? "http://localhost:4200/subscription/failure",
                Pending = _config["MercadoPago:PendingUrl"] ?? "http://localhost:4200/subscription/pending",
            },
            AutoReturn = string.IsNullOrEmpty(_config["MercadoPago:SuccessUrl"]) || _config["MercadoPago:SuccessUrl"]!.Contains("localhost") ? null : "approved",
            ExternalReference = $"{userId}|{planId}",
            NotificationUrl = _config["MercadoPago:WebhookUrl"],
        };

        Preference preference = await client.CreateAsync(request);
        return preference.InitPoint;
    }

    public async Task HandlePaymentNotificationAsync(string topic, long id, CancellationToken ct)
    {
        if (topic != "payment")
            return;

        var paymentClient = new PaymentClient();
        Payment payment = await paymentClient.GetAsync(id);

        if (payment == null)
        {
            _logger.LogWarning("Payment {PaymentId} not found in MercadoPago", id);
            return;
        }

        var externalRef = payment.ExternalReference;
        if (string.IsNullOrEmpty(externalRef))
            return;

        var parts = externalRef.Split('|');
        if (parts.Length != 2 || !Guid.TryParse(parts[0], out var userId) || !Guid.TryParse(parts[1], out var planId))
            return;

        _logger.LogInformation("Processing payment {PaymentId} status={Status} for user={UserId}", id, payment.Status, userId);

        if (payment.Status == "approved")
        {
            var subscription = await _subscriptionRepo.GetActiveByUserIdAsync(userId, ct);
            if (subscription != null)
            {
                subscription.PlanId = planId;
                subscription.Status = SubscriptionStatus.Active;
                subscription.MpPayerId = payment.Payer?.Id?.ToString();
                subscription.MpSubscriptionId = payment.Id?.ToString();
                subscription.StartDate = DateTime.UtcNow;
                subscription.EndDate = DateTime.UtcNow.AddMonths(1);
                _subscriptionRepo.UpdateSubscription(subscription);
                await _unitOfWork.SaveChangesAsync(ct);
            }
        }
    }
}
