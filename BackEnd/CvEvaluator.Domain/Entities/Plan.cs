namespace CvEvaluator.Domain.Entities;

public class Plan
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MaxEvaluationsPerMonth { get; set; }  // -1 = unlimited
    public int MaxJobPositions { get; set; }          // -1 = unlimited
    public decimal Price { get; set; }
    public string Currency { get; set; } = "ARS";

    public ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
}
