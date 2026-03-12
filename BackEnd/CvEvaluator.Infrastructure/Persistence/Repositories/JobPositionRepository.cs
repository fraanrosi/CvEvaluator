using CvEvaluator.Application.Interfaces;
using CvEvaluator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CvEvaluator.Infrastructure.Persistence.Repositories;

public class JobPositionRepository : IJobPositionRepository
{
    private readonly CvEvaluatorDbContext _context;

    public JobPositionRepository(CvEvaluatorDbContext context)
    {
        _context = context;
    }

    public async Task<JobPosition?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.JobPositions
            .FirstOrDefaultAsync(j => j.Id == id, ct);
    }

    public async Task<JobPosition?> GetByIdWithEvaluationsAsync(Guid id, CancellationToken ct)
    {
        return await _context.JobPositions
            .Include(j => j.Evaluations)
            .FirstOrDefaultAsync(j => j.Id == id, ct);
    }

    public async Task<IEnumerable<JobPosition>> GetByUserIdAsync(Guid userId, CancellationToken ct)
    {
        return await _context.JobPositions
            .Include(j => j.Evaluations)
            .Where(j => j.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task AddAsync(JobPosition jobPosition, CancellationToken ct)
    {
        await _context.JobPositions.AddAsync(jobPosition, ct);
    }

    public void Update(JobPosition jobPosition)
    {
        _context.JobPositions.Update(jobPosition);
    }

    public void Delete(JobPosition jobPosition)
    {
        _context.JobPositions.Remove(jobPosition);
    }

    public async Task<int> CountByUserIdAsync(Guid userId, CancellationToken ct)
        => await _context.JobPositions.CountAsync(j => j.UserId == userId, ct);
}