namespace CvEvaluator.Application.Interfaces;

public interface IEvaluationQueue
{
    ValueTask EnqueueAsync(Guid evaluationId);
}