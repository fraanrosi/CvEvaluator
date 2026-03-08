using CvEvaluator.Application.DTOs;

namespace CvEvaluator.Application.Interfaces;

public interface IJobPositionService
{
    Task<IEnumerable<JobPositionDto>> GetAllAsync(Guid userId, CancellationToken ct);
    Task<JobPositionDetailDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct);
    Task<JobPositionDto> CreateAsync(CreateJobPositionDto dto, Guid userId, CancellationToken ct);
    Task<bool> UpdateAsync(Guid id, UpdateJobPositionDto dto, Guid userId, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct);
}
