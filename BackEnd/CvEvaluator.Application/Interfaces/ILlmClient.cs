namespace CvEvaluator.Application.Interfaces;

public interface ILlmClient
{
    Task<string> EvaluateCvAsync(string prompt, CancellationToken ct);
}