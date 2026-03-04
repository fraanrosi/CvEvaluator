using CvEvaluator.Domain.Entities;
using CvEvaluator.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CvEvaluator.Infrastructure.Persistence;

public class CvEvaluatorDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public CvEvaluatorDbContext(DbContextOptions<CvEvaluatorDbContext> options)
        : base(options)
    {
    }

    public DbSet<CvEvaluation> Evaluations => Set<CvEvaluation>();
    public DbSet<JobPosition> JobPositions => Set<JobPosition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureJobPosition(modelBuilder);
        ConfigureEvaluation(modelBuilder);
    }

    private static void ConfigureJobPosition(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobPosition>(entity =>
        {
            entity.HasKey(j => j.Id);

            entity.Property(j => j.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(j => j.Description)
                .IsRequired();

            entity.Property(j => j.CreatedAt)
                .IsRequired();

            entity.HasIndex(j => j.UserId);
            entity.HasIndex(j => j.CreatedAt);

            // Relación con Identity
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(j => j.UserId)
                .OnDelete(DeleteBehavior.Cascade);
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

            entity.Property(e => e.EvaluationResult)
                .HasColumnType("jsonb");

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.HasIndex(e => e.FileHash);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.JobPositionId);

            // Relación con Identity
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔥 NUEVA relación con JobPosition
            entity.HasOne(e => e.JobPosition)
                .WithMany(j => j.Evaluations)
                .HasForeignKey(e => e.JobPositionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}