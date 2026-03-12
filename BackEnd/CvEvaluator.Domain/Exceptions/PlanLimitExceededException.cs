namespace CvEvaluator.Domain.Exceptions;

public class PlanLimitExceededException : Exception
{
    public string LimitType { get; }

    public PlanLimitExceededException(string limitType, string message)
        : base(message)
    {
        LimitType = limitType;
    }
}
