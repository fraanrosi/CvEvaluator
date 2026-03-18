using CvEvaluator.Domain.Entities;

namespace CvEvaluator.Application.Interfaces;

public interface ISubscriptionRepository
{
    Task<IEnumerable<Plan>> GetAllPlansAsync(CancellationToken ct);
    Task<Plan?> GetPlanByNameAsync(string name, CancellationToken ct);
    Task<UserSubscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct);
    Task AddSubscriptionAsync(UserSubscription subscription, CancellationToken ct);
    Task<MonthlyUsageCounter?> GetCounterAsync(Guid userId, int year, int month, CancellationToken ct);
    Task AddCounterAsync(MonthlyUsageCounter counter, CancellationToken ct);
    void UpdateCounter(MonthlyUsageCounter counter);
    Task<Plan?> GetPlanByIdAsync(Guid planId, CancellationToken ct);
    Task<UserSubscription?> GetByMpSubscriptionIdAsync(string mpSubscriptionId, CancellationToken ct);
    void UpdateSubscription(UserSubscription subscription);
}
