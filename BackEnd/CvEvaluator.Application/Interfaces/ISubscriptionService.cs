using CvEvaluator.Application.DTOs;

namespace CvEvaluator.Application.Interfaces;

public interface ISubscriptionService
{
    Task<IEnumerable<PlanDto>> GetAllPlansAsync(CancellationToken ct);
    Task<UserSubscriptionDto> GetUserSubscriptionAsync(Guid userId, CancellationToken ct);
    Task AssignFreePlanAsync(Guid userId, CancellationToken ct);
    Task CheckEvaluationLimitAsync(Guid userId, CancellationToken ct);
    Task CheckJobPositionLimitAsync(Guid userId, CancellationToken ct);
    Task IncrementEvaluationCountAsync(Guid userId, CancellationToken ct);
}
