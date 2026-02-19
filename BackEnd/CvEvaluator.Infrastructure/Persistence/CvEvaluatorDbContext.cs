using CvEvaluator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CvEvaluator.Infrastructure.Persistence;

public class CvEvaluatorDbContext : DbContext
{
    public CvEvaluatorDbContext(DbContextOptions<CvEvaluatorDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<CvEvaluation> Evaluations => Set<CvEvaluation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureEvaluation(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.PasswordHash)
                .IsRequired();

            entity.HasIndex(u => u.Email)
                .IsUnique();
        });
    }

    private static void ConfigureEvaluation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CvEvaluation>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.OriginalFilename)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.FileHash)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(e => e.ExtractedText)
                .IsRequired();

            // JSONB en PostgreSQL 🔥
            entity.Property(e => e.EvaluationResult)
                .HasColumnType("jsonb");

            entity.Property(e => e.Status)
                .HasConversion<string>() // recomendable
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.HasOne(e => e.User)
                .WithMany(u => u.Evaluations)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.FileHash);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
