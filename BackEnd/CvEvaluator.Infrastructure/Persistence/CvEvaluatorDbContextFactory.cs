using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CvEvaluator.Infrastructure.Persistence;

public class CvEvaluatorDbContextFactory
    : IDesignTimeDbContextFactory<CvEvaluatorDbContext>
{
    public CvEvaluatorDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.GetFullPath(
            Path.Combine(Directory.GetCurrentDirectory(), "../CvEvaluator.API"));

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString =
            configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
            throw new Exception("Connection string not found.");

        var optionsBuilder =
            new DbContextOptionsBuilder<CvEvaluatorDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new CvEvaluatorDbContext(optionsBuilder.Options);
    }
}