namespace CvEvaluator.Domain.Entities;

public class CandidateProfile
{
    public string FullName { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public bool HasDotNetExperience { get; set; }
    public bool HasDatabaseKnowledge { get; set; }
}
