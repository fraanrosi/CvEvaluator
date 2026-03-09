using CvEvaluator.Application.DTOs;
using CvEvaluator.Application.Interfaces;
using CvEvaluator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CvEvaluator.Infrastructure.Persistence.Repositories;

public class CvEvaluationRepository : ICvEvaluationRepository
{
    private readonly CvEvaluatorDbContext _context;

    public CvEvaluationRepository(CvEvaluatorDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CvEvaluation evaluation, CancellationToken ct)
    {
        await _context.Evaluations.AddAsync(evaluation, ct);
    }

    public async Task<IEnumerable<CvEvaluation>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
    {
        return await _context.Evaluations
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<CvEvaluation?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Evaluations
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public void Update(CvEvaluation evaluation)
    {
        _context.Evaluations.Update(evaluation);
    }
}
