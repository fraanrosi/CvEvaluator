using CvEvaluator.Domain.Enums;

namespace CvEvaluator.Domain.Entities;

public class UserSubscription
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public Plan Plan { get; set; } = null!;
    public SubscriptionStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? MpPayerId { get; set; }
    public string? MpSubscriptionId { get; set; }
    public string? MpPreapprovalId { get; set; }
}
