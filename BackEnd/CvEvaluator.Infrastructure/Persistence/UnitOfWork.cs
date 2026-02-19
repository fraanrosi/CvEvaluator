using CvEvaluator.Application.Interfaces;

namespace CvEvaluator.Infrastructure.Persistence;
public class UnitOfWork : IUnitOfWork
{
    private readonly CvEvaluatorDbContext _context;

    public UnitOfWork(CvEvaluatorDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return _context.SaveChangesAsync(ct);
    }
}
