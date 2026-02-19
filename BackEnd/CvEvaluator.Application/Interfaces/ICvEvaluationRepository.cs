using CvEvaluator.Domain.Entities;

namespace CvEvaluator.Application.Interfaces;

public interface ICvEvaluationRepository
{
    Task AddAsync(CvEvaluation evaluation, CancellationToken ct);
    Task<CvEvaluation?> GetByIdAsync(Guid id, CancellationToken ct);
    void Update(CvEvaluation evaluation);
}
