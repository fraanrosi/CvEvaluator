namespace CvEvaluator.Domain.Models;

public class CvEvaluationResult
{
    public int Score { get; set; }
    public int YearsExperience { get; set; }
    public bool MatchesRequirements { get; set; }
    public List<string> Strengths { get; set; }
    public List<string> Weaknesses { get; set; }
}
