namespace CvEvaluator.Application.Interfaces;

public interface IPaymentService
{
    Task<string> CreateCheckoutAsync(Guid userId, string userEmail, Guid planId, CancellationToken ct);
    Task HandlePaymentNotificationAsync(string topic, long id, CancellationToken ct);
}
