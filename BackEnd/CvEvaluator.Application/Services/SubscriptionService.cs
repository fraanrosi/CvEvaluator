using CvEvaluator.Application.DTOs;
using CvEvaluator.Application.Interfaces;
using CvEvaluator.Domain.Entities;
using CvEvaluator.Domain.Enums;
using CvEvaluator.Domain.Exceptions;

namespace CvEvaluator.Application.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IJobPositionRepository _jobPositionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubscriptionService(
        ISubscriptionRepository subscriptionRepository,
        IJobPositionRepository jobPositionRepository,
        IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _jobPositionRepository = jobPositionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<PlanDto>> GetAllPlansAsync(CancellationToken ct)
    {
        var plans = await _subscriptionRepository.GetAllPlansAsync(ct);
        return plans.Select(p => new PlanDto
        {
            Id = p.Id,
            Name = p.Name,
            MaxEvaluationsPerMonth = p.MaxEvaluationsPerMonth,
            MaxJobPositions = p.MaxJobPositions,
            Price = p.Price,
            Currency = p.Currency
        });
    }

    public async Task<UserSubscriptionDto> GetUserSubscriptionAsync(Guid userId, CancellationToken ct)
    {
        var subscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId, ct);

        if (subscription is null)
        {
            await AssignFreePlanAsync(userId, ct);
            subscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId, ct)
                ?? throw new InvalidOperationException("Failed to assign free plan");
        }

        var now = DateTime.UtcNow;
        var counter = await _subscriptionRepository.GetCounterAsync(userId, now.Year, now.Month, ct);
        var jobPositionsCount = await _jobPositionRepository.CountByUserIdAsync(userId, ct);

        return new UserSubscriptionDto
        {
            PlanId = subscription.PlanId,
            PlanName = subscription.Plan.Name,
            MaxEvaluationsPerMonth = subscription.Plan.MaxEvaluationsPerMonth,
            MaxJobPositions = subscription.Plan.MaxJobPositions,
            Status = subscription.Status,
            StartDate = subscription.StartDate,
            EvaluationsUsedThisMonth = counter?.EvaluationCount ?? 0,
            JobPositionsCount = jobPositionsCount
        };
    }

    public async Task AssignFreePlanAsync(Guid userId, CancellationToken ct)
    {
        var freePlan = await _subscriptionRepository.GetPlanByNameAsync("Free", ct)
            ?? throw new InvalidOperationException("Free plan not found in database");

        var subscription = new UserSubscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlanId = freePlan.Id,
            Status = SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow
        };

        await _subscriptionRepository.AddSubscriptionAsync(subscription, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task CheckEvaluationLimitAsync(Guid userId, CancellationToken ct)
    {
        var subscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId, ct);

        if (subscription is null)
        {
            await AssignFreePlanAsync(userId, ct);
            subscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId, ct)
                ?? throw new InvalidOperationException("Failed to assign free plan");
        }

        var limit = subscription.Plan.MaxEvaluationsPerMonth;
        if (limit == -1) return;

        var now = DateTime.UtcNow;
        var counter = await _subscriptionRepository.GetCounterAsync(userId, now.Year, now.Month, ct);
        var used = counter?.EvaluationCount ?? 0;

        if (used >= limit)
            throw new PlanLimitExceededException(
                "evaluations",
                $"You have reached the limit of {limit} evaluations per month on the {subscription.Plan.Name} plan.");
    }

    public async Task CheckJobPositionLimitAsync(Guid userId, CancellationToken ct)
    {
        var subscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId, ct);

        if (subscription is null)
        {
            await AssignFreePlanAsync(userId, ct);
            subscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId, ct)
                ?? throw new InvalidOperationException("Failed to assign free plan");
        }

        var limit = subscription.Plan.MaxJobPositions;
        if (limit == -1) return;

        var count = await _jobPositionRepository.CountByUserIdAsync(userId, ct);

        if (count >= limit)
            throw new PlanLimitExceededException(
                "jobPositions",
                $"You have reached the limit of {limit} job positions on the {subscription.Plan.Name} plan.");
    }

    public async Task IncrementEvaluationCountAsync(Guid userId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var counter = await _subscriptionRepository.GetCounterAsync(userId, now.Year, now.Month, ct);

        if (counter is null)
        {
            counter = new MonthlyUsageCounter
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Year = now.Year,
                Month = now.Month,
                EvaluationCount = 1
            };
            await _subscriptionRepository.AddCounterAsync(counter, ct);
        }
        else
        {
            counter.EvaluationCount++;
            _subscriptionRepository.UpdateCounter(counter);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
