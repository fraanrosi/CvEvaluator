using CvEvaluator.Application.DTOs;
using CvEvaluator.Application.Interfaces;
using CvEvaluator.Domain.Entities;

namespace CvEvaluator.Application.Services;

public class JobPositionService : IJobPositionService
{
    private readonly IJobPositionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public JobPositionService(IJobPositionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<JobPositionDto>> GetAllAsync(Guid userId, CancellationToken ct)
    {
        var positions = await _repository.GetByUserIdAsync(userId, ct);

        return positions.Select(p => new JobPositionDto
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            CreatedAt = p.CreatedAt,
            EvaluationsCount = p.Evaluations.Count
        });
    }

    public async Task<JobPositionDetailDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct)
    {
        var position = await _repository.GetByIdWithEvaluationsAsync(id, ct);

        if (position == null || position.UserId != userId)
            return null;

        return new JobPositionDetailDto
        {
            Id = position.Id,
            Title = position.Title,
            Description = position.Description,
            CreatedAt = position.CreatedAt,
            Evaluations = position.Evaluations.Select(e => new EvaluationSummaryDto
            {
                Id = e.Id,
                FileName = e.OriginalFilename,
                CandidateName = Path.GetFileNameWithoutExtension(e.OriginalFilename),
                Score = e.OverallScore,
                EvaluatedAt = e.EvaluatedAt
            }).ToList()
        };
    }

    public async Task<JobPositionDto> CreateAsync(CreateJobPositionDto dto, Guid userId, CancellationToken ct)
    {
        var position = new JobPosition
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = dto.Title,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(position, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new JobPositionDto
        {
            Id = position.Id,
            Title = position.Title,
            Description = position.Description,
            CreatedAt = position.CreatedAt,
            EvaluationsCount = 0
        };
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateJobPositionDto dto, Guid userId, CancellationToken ct)
    {
        var position = await _repository.GetByIdAsync(id, ct);

        if (position == null || position.UserId != userId)
            return false;

        position.Title = dto.Title;
        position.Description = dto.Description;

        _repository.Update(position);
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct)
    {
        var position = await _repository.GetByIdAsync(id, ct);

        if (position == null || position.UserId != userId)
            return false;

        _repository.Delete(position);
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}
