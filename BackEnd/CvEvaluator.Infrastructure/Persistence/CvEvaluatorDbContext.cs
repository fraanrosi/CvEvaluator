using CvEvaluator.Domain.Entities;
using CvEvaluator.Domain.Enums;
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
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
    public DbSet<MonthlyUsageCounter> MonthlyUsageCounters => Set<MonthlyUsageCounter>();

    private static readonly Guid FreePlanId     = new("00000000-0000-0000-0000-000000000001");
    private static readonly Guid ProPlanId      = new("00000000-0000-0000-0000-000000000002");
    private static readonly Guid BusinessPlanId = new("00000000-0000-0000-0000-000000000003");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureJobPosition(modelBuilder);
        ConfigureEvaluation(modelBuilder);
        ConfigurePlan(modelBuilder);
        ConfigureUserSubscription(modelBuilder);
        ConfigureMonthlyUsageCounter(modelBuilder);
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

    private static void ConfigurePlan(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Plan>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(p => p.Price).HasColumnType("decimal(10,2)");
            entity.Property(p => p.Currency).HasMaxLength(10).HasDefaultValue("ARS");

            entity.HasData(
                new Plan { Id = FreePlanId,     Name = "Free",     MaxEvaluationsPerMonth = 5,  MaxJobPositions = 2,  Price = 0,     Currency = "ARS" },
                new Plan { Id = ProPlanId,      Name = "Pro",      MaxEvaluationsPerMonth = 50, MaxJobPositions = 20, Price = 9999,  Currency = "ARS" },
                new Plan { Id = BusinessPlanId, Name = "Business", MaxEvaluationsPerMonth = -1, MaxJobPositions = -1, Price = 24999, Currency = "ARS" }
            );
        });
    }

    private static void ConfigureUserSubscription(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserSubscription>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.Status)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(s => s.MpPayerId).HasMaxLength(100);
            entity.Property(s => s.MpSubscriptionId).HasMaxLength(100);
            entity.Property(s => s.MpPreapprovalId).HasMaxLength(100);

            entity.HasIndex(s => s.UserId);

            entity.HasOne(s => s.Plan)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(s => s.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureMonthlyUsageCounter(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MonthlyUsageCounter>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.HasIndex(c => new { c.UserId, c.Year, c.Month })
                .IsUnique();

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(c => c.UserId)
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