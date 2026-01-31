using CvEvaluator.Domain.Enums;
using CvEvaluator.Domain.Models;

namespace CvEvaluator.Application.Interfaces;

public interface ICvEvaluationService
{
    Task<(CvDecision Decision, CvEvaluationResult Result)> ExecuteAsync(string cvText);
}
