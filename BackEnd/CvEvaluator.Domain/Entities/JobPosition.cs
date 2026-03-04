namespace CvEvaluator.Domain.Entities;

public class JobPosition
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public ICollection<CvEvaluation> Evaluations { get; set; } = new List<CvEvaluation>();
}