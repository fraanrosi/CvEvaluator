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

    public async Task<CvEvaluation?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Evaluations
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public void Update(CvEvaluation evaluation)
    {
        _context.Evaluations.Update(evaluation);
    }
}
