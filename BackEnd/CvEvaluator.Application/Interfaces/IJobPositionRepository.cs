using CvEvaluator.Domain.Entities;

namespace CvEvaluator.Application.Interfaces;

public interface IJobPositionRepository
{
    Task<JobPosition?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<JobPosition?> GetByIdWithEvaluationsAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<JobPosition>> GetByUserIdAsync(Guid userId, CancellationToken ct);
    Task AddAsync(JobPosition jobPosition, CancellationToken ct);
    void Update(JobPosition jobPosition);
    void Delete(JobPosition jobPosition);
    Task<int> CountByUserIdAsync(Guid userId, CancellationToken ct);
}