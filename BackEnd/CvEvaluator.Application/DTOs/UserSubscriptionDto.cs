using CvEvaluator.Domain.Enums;

namespace CvEvaluator.Application.DTOs;

public class UserSubscriptionDto
{
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int MaxEvaluationsPerMonth { get; set; }
    public int MaxJobPositions { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public int EvaluationsUsedThisMonth { get; set; }
    public int JobPositionsCount { get; set; }
}
