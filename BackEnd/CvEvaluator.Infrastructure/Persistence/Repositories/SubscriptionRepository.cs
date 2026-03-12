using CvEvaluator.Application.Interfaces;
using CvEvaluator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CvEvaluator.Infrastructure.Persistence.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly CvEvaluatorDbContext _context;

    public SubscriptionRepository(CvEvaluatorDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Plan>> GetAllPlansAsync(CancellationToken ct)
        => await _context.Plans.ToListAsync(ct);

    public async Task<Plan?> GetPlanByNameAsync(string name, CancellationToken ct)
        => await _context.Plans.FirstOrDefaultAsync(p => p.Name == name, ct);

    public async Task<UserSubscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        => await _context.UserSubscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.UserId == userId, ct);

    public async Task AddSubscriptionAsync(UserSubscription subscription, CancellationToken ct)
        => await _context.UserSubscriptions.AddAsync(subscription, ct);

    public async Task<MonthlyUsageCounter?> GetCounterAsync(Guid userId, int year, int month, CancellationToken ct)
        => await _context.MonthlyUsageCounters
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Year == year && c.Month == month, ct);

    public async Task AddCounterAsync(MonthlyUsageCounter counter, CancellationToken ct)
        => await _context.MonthlyUsageCounters.AddAsync(counter, ct);

    public void UpdateCounter(MonthlyUsageCounter counter)
        => _context.MonthlyUsageCounters.Update(counter);
}
