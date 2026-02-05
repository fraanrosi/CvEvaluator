namespace CvEvaluator.Domain.Models;

public class CvEvaluationResult
{
    public int Score { get; set; }
    public int YearsExperience { get; set; }
    public bool MatchesRequirements { get; set; }
    public required List<string> Strengths { get; set; }
    public required List<string> Weaknesses { get; set; }
}
