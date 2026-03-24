using CvEvaluator.Application.DTOs;
using CvEvaluator.Application.Interfaces;
using CvEvaluator.Application.Services;
using CvEvaluator.Domain.Entities;
using CvEvaluator.Domain.Enums;
using CvEvaluator.Domain.Exceptions;
using Moq;

namespace CvEvaluator.Application.Tests;

public class SubscriptionServiceTests
{
    private readonly Mock<ISubscriptionRepository> _subRepoMock;
    private readonly Mock<IJobPositionRepository> _jobRepoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly SubscriptionService _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public SubscriptionServiceTests()
    {
        _subRepoMock = new Mock<ISubscriptionRepository>();
        _jobRepoMock = new Mock<IJobPositionRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _sut = new SubscriptionService(_subRepoMock.Object, _jobRepoMock.Object, _uowMock.Object);
    }

    private Plan CreatePlan(string name = "Pro", int maxEvals = 50, int maxJobs = 10, decimal price = 9.99m)
        => new() { Id = Guid.NewGuid(), Name = name, MaxEvaluationsPerMonth = maxEvals, MaxJobPositions = maxJobs, Price = price, Currency = "ARS" };

    private UserSubscription CreateSubscription(Plan plan)
        => new() { Id = Guid.NewGuid(), UserId = _userId, PlanId = plan.Id, Plan = plan, Status = SubscriptionStatus.Active, StartDate = DateTime.UtcNow };

    // ─── GetAllPlansAsync ────────────────────────────────────────────

    [Fact]
    public async Task GetAllPlansAsync_ReturnsMappedPlanDtos()
    {
        var plans = new List<Plan> { CreatePlan("Free", 5, 2, 0), CreatePlan("Pro", 50, 10, 9.99m) };
        _subRepoMock.Setup(r => r.GetAllPlansAsync(default)).ReturnsAsync(plans);

        var result = (await _sut.GetAllPlansAsync(default)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Free", result[0].Name);
        Assert.Equal(50, result[1].MaxEvaluationsPerMonth);
    }

    [Fact]
    public async Task GetAllPlansAsync_EmptyList_ReturnsEmpty()
    {
        _subRepoMock.Setup(r => r.GetAllPlansAsync(default)).ReturnsAsync(new List<Plan>());

        var result = await _sut.GetAllPlansAsync(default);

        Assert.Empty(result);
    }

    // ─── GetUserSubscriptionAsync ────────────────────────────────────

    [Fact]
    public async Task GetUserSubscriptionAsync_HasActiveSubscription_ReturnsMappedDto()
    {
        var plan = CreatePlan("Pro", 50, 10);
        var sub = CreateSubscription(plan);
        var counter = new MonthlyUsageCounter { UserId = _userId, Year = DateTime.UtcNow.Year, Month = DateTime.UtcNow.Month, EvaluationCount = 3 };

        _subRepoMock.Setup(r => r.GetActiveByUserIdAsync(_userId, default)).ReturnsAsync(sub);
        _subRepoMock.Setup(r => r.GetCounterAsync(_userId, It.IsAny<int>(), It.IsAny<int>(), default)).ReturnsAsync(counter);
        _jobRepoMock.Setup(r => r.CountByUserIdAsync(_userId, default)).ReturnsAsync(5);

        var result = await _sut.GetUserSubscriptionAsync(_userId, default);

        Assert.Equal("Pro", result.PlanName);
        Assert.Equal(3, result.EvaluationsUsedThisMonth);
        Assert.Equal(5, result.JobPositionsCount);
        Assert.Equal(SubscriptionStatus.Active, result.Status);
    }

    [Fact]
    public async Task GetUserSubscriptionAsync_NoSubscription_AutoAssignsFreePlan()
    {
        var freePlan = CreatePlan("Free", 5, 2, 0);
        var sub = CreateSubscription(freePlan);

        // First call returns null, second call returns the new subscription
        _subRepoMock.SetupSequence(r => r.GetActiveByUserIdAsync(_userId, default))
            .ReturnsAsync((UserSubscription?)null)
            .ReturnsAsync(sub);
        _subRepoMock.Setup(r => r.GetPlanByNameAsync("Free", default)).ReturnsAsync(freePlan);
        _subRepoMock.Setup(r => r.GetCounterAsync(_userId, It.IsAny<int>(), It.IsAny<int>(), default)).ReturnsAsync((MonthlyUsageCounter?)null);
        _jobRepoMock.Setup(r => r.CountByUserIdAsync(_userId, default)).ReturnsAsync(0);

        var result = await _sut.GetUserSubscriptionAsync(_userId, default);

        Assert.Equal("Free", result.PlanName);
        _subRepoMock.Verify(r => r.AddSubscriptionAsync(It.IsAny<UserSubscription>(), default), Times.Once);
    }

    [Fact]
    public async Task GetUserSubscriptionAsync_FreePlanNotFound_ThrowsInvalidOperationException()
    {
        _subRepoMock.Setup(r => r.GetActiveByUserIdAsync(_userId, default)).ReturnsAsync((UserSubscription?)null);
        _subRepoMock.Setup(r => r.GetPlanByNameAsync("Free", default)).ReturnsAsync((Plan?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.GetUserSubscriptionAsync(_userId, default));
    }

    [Fact]
    public async Task GetUserSubscriptionAsync_NoCounterExists_ReturnsZeroEvaluationsUsed()
    {
        var plan = CreatePlan();
        var sub = CreateSubscription(plan);

        _subRepoMock.Setup(r => r.GetActiveByUserIdAsync(_userId, default)).ReturnsAsync(sub);
        _subRepoMock.Setup(r => r.GetCounterAsync(_userId, It.IsAny<int>(), It.IsAny<int>(), default)).ReturnsAsync((MonthlyUsageCounter?)null);
        _jobRepoMock.Setup(r => r.CountByUserIdAsync(_userId, default)).ReturnsAsync(0);

        var result = await _sut.GetUserSubscriptionAsync(_userId, default);

        Assert.Equal(0, result.EvaluationsUsedThisMonth);
    }

    [Fact]
    public async Task GetUserSubscriptionAsync_HasCounter_ReturnsCorrectCount()
    {
        var plan = CreatePlan();
        var sub = CreateSubscription(plan);
        var counter = new MonthlyUsageCounter { EvaluationCount = 7 };

        _subRepoMock.Setup(r => r.GetActiveByUserIdAsync(_userId, default)).ReturnsAsync(sub);
        _subRepoMock.Setup(r => r.GetCounterAsync(_userId, It.IsAny<int>(), It.IsAny<int>(), default)).ReturnsAsync(counter);
        _jobRepoMock.Setup(r => r.CountByUserIdAsync(_userId, default)).ReturnsAsync(2);

        var result = await _sut.GetUserSubscriptionAsync(_userId, default);

        Assert.Equal(7, result.EvaluationsUsedThisMonth);
    }

    // ─── AssignFreePlanAsync ─────────────────────────────────────────

    [Fact]
    public async Task AssignFreePlanAsync_FreePlanNotFound_ThrowsInvalidOperationException()
    {
        _subRepoMock.Setup(r => r.GetPlanByNameAsync("Free", default)).ReturnsAsync((Plan?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AssignFreePlanAsync(_userId, default));
    }

    [Fact]
    public async Task AssignFreePlanAsync_CreatesActiveSubscriptionWithCorrectFields()
    {
        var freePlan = CreatePlan("Free", 5, 2, 0);
        _subRepoMock.Setup(r => r.GetPlanByNameAsync("Free", default)).ReturnsAsync(freePlan);

        UserSubscription? captured = null;
        _subRepoMock.Setup(r => r.AddSubscriptionAsync(It.IsAny<UserSubscription>(), default))
            .Callback<UserSubscription, CancellationToken>((s, _) => captured = s);

        await _sut.AssignFreePlanAsync(_userId, default);

        Assert.NotNull(captured);
        Assert.Equal(_userId, captured!.UserId);
        Assert.Equal(freePlan.Id, captured.PlanId);
        Assert.Equal(SubscriptionStatus.Active, captured.Status);
    }

    [Fact]
    public async Task AssignFreePlanAsync_CallsSaveChanges()
    {
        var freePlan = CreatePlan("Free", 5, 2, 0);
        _subRepoMock.Setup(r => r.GetPlanByNameAsync("Free", default)).ReturnsAsync(freePlan);

        await _sut.AssignFreePlanAsync(_userId, default);

        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    // ─── CheckEvaluationLimitAsync ───────────────────────────────────

    [Fact]
    public async Task CheckEvaluationLimitAsync_NoSubscription_AutoAssignsFreePlan()
    {
        var freePlan = CreatePlan("Free", 5, 2, 0);
        var sub = CreateSubscription(freePlan);

        _subRepoMock.SetupSequence(r => r.GetActiveByUserIdAsync(_userId, default))
            .ReturnsAsync((UserSubscription?)null)
            .ReturnsAsync(sub);
        _subRepoMock.Setup(r => r.GetPlanByNameAsync("Free", default)).ReturnsAsync(freePlan);
        _subRepoMock.Setup(r => r.GetCounterAsync(_userId, It.IsAny<int>(), It.IsAny<int>(), default)).ReturnsAsync((MonthlyUsageCounter?)null);

        await _sut.CheckEvaluationLimitAsync(_userId, default);

        _subRepoMock.Verify(r => r.AddSubscriptionAsync(It.IsAny<UserSubscription>(), default), Times.Once);
    }

    [Fact]
    public async Task CheckEvaluationLimitAsync_UnlimitedPlan_DoesNotThrow()
    {
        var plan = CreatePlan("Enterprise", maxEvals: -1);
        var sub = CreateSubscription(plan);
        _subRepoMock.Setup(r => r.GetActiveByUserIdAsync(_userId, default)).ReturnsAsync(sub);

        await _sut.CheckEvaluationLimitAsync(_userId, default);

        // Should not call GetCounterAsync since limit is -1
        _subRepoMock.Verify(r => r.GetCounterAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckEvaluationLimitAsync_UnderLimit_DoesNotThrow()
    {
        var plan = CreatePlan("Pro", maxEvals: 50);
        var sub = CreateSubscription(plan);
        var counter = new MonthlyUsageCounter { EvaluationCount = 10 };

        _subRepoMock.Setup(r => r.GetActiveByUserIdAsync(_userId, default)).ReturnsAsync(sub);
        _subRepoMock.Setup(r => r.GetCounterAsync(_userId, It.IsAny<int>(), It.IsAny<int>(), default)).ReturnsAsync(counter);

        var exception = await Record.ExceptionAsync(() => _sut.CheckEvaluationLimitAsync(_userId, default));

        Assert.Null(exception);
    }

    [Fact]
    public async Task CheckEvaluationLimitAsync_AtLimit_ThrowsPlanLimitExceededException()
    {
        var plan = CreatePlan("Free", maxEvals: 5);
        var sub = CreateSubscription(plan);
        var counter = new MonthlyUsageCounter { EvaluationCount = 5 };

        _subRepoMock.Setup(r => r.GetActiveByUserIdAsync(_userId, default)).ReturnsAsync(sub);
        _subRepoMock.Setup(r => r.GetCounterAsync(_userId, It.IsAny<int>(), It.IsAny<int>(), default)).ReturnsAsync(counter);

        var ex = await Assert.ThrowsAsync<PlanLimitExceededException>(() => _sut.CheckEvaluationLimitAsync(_userId, default));
        Assert.Equal("evaluations", ex.LimitType);
    }

    [Fact]
    public async Task CheckEvaluationLimitAsync_OverLimit_ThrowsPlanLimitExceededException()
    {
        var plan = CreatePlan("Free", maxEvals: 5);
        var sub = CreateSubscription(plan);
        var counter = new MonthlyUsageCounter { EvaluationCount = 10 };

        _subRepoMock.Setup(r => r.GetActiveByUserIdAsync(_userId, default)).ReturnsAsync(sub);
        _subRepoMock.Setup(r => r.GetCounterAsync(_userId, It.IsAny<int>(), It.IsAny<int>(), default)).ReturnsAsync(counter);

        await Assert.ThrowsAsync<PlanLimitExceededException>(() => _sut.CheckEvaluationLimitAsync(_userId, default));
    }

    [Fact]
    public async Task CheckEvaluationLimitAsync_NoCounter_TreatsAsZeroUsed()
    {
        var plan = CreatePlan("Free", maxEvals: 5);
        var sub = CreateSubscription(plan);

        _subRepoMock.Setup(r => r.GetActiveByUserIdAsync(_userId, default)).ReturnsAsync(sub);
        _subRepoMock.Setup(r => r.GetCounterAsync(_userId, It.IsAny<int>(), It.IsAny<int>(), default)).ReturnsAsync((MonthlyUsageCounter?)null);

        var exception = await Record.ExceptionAsync(() => _sut.CheckEvaluationLimitAsync(_userId, default));

        Assert.Null(exception);
    }

    // ─── CheckJobPositionLimitAsync ──────────────────────────────────

    [Fact]
    public async Task CheckJobPositionLimitAsync_UnlimitedPlan_DoesNotThrow()
    {
        var plan = CreatePlan("Enterprise", maxJobs: -1);
        var sub = CreateSubscription(plan);
        _subRepoMock.Setup(r => r.GetActiveByUserIdAsync(_userId, default)).ReturnsAsync(sub);

        await _sut.CheckJobPositionLimitAsync(_userId, default);

        _jobRepoMock.Verify(r => r.CountByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckJobPositionLimitAsync_UnderLimit_DoesNotThrow()
    {
        var plan = CreatePlan("Pro", maxJobs: 10);
        var sub = CreateSubscription(plan);

        _subRepoMock.Setup(r => r.GetActiveByUserIdAsync(_userId, default)).ReturnsAsync(sub);
        _jobRepoMock.Setup(r => r.CountByUserIdAsync(_userId, default)).ReturnsAsync(3);

        var exception = await Record.ExceptionAsync(() => _sut.CheckJobPositionLimitAsync(_userId, default));

        Assert.Null(exception);
    }

    [Fact]
    public async Task CheckJobPositionLimitAsync_AtLimit_ThrowsPlanLimitExceededException()
    {
        var plan = CreatePlan("Free", maxJobs: 2);
        var sub = CreateSubscription(plan);

        _subRepoMock.Setup(r => r.GetActiveByUserIdAsync(_userId, default)).ReturnsAsync(sub);
        _jobRepoMock.Setup(r => r.CountByUserIdAsync(_userId, default)).ReturnsAsync(2);

        var ex = await Assert.ThrowsAsync<PlanLimitExceededException>(() => _sut.CheckJobPositionLimitAsync(_userId, default));
        Assert.Equal("jobPositions", ex.LimitType);
    }

    // ─── IncrementEvaluationCountAsync ───────────────────────────────

    [Fact]
    public async Task IncrementEvaluationCountAsync_NoCounter_CreatesNewCounterWithOne()
    {
        _subRepoMock.Setup(r => r.GetCounterAsync(_userId, It.IsAny<int>(), It.IsAny<int>(), default)).ReturnsAsync((MonthlyUsageCounter?)null);

        MonthlyUsageCounter? captured = null;
        _subRepoMock.Setup(r => r.AddCounterAsync(It.IsAny<MonthlyUsageCounter>(), default))
            .Callback<MonthlyUsageCounter, CancellationToken>((c, _) => captured = c);

        await _sut.IncrementEvaluationCountAsync(_userId, default);

        Assert.NotNull(captured);
        Assert.Equal(1, captured!.EvaluationCount);
        Assert.Equal(_userId, captured.UserId);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task IncrementEvaluationCountAsync_ExistingCounter_IncrementsCount()
    {
        var counter = new MonthlyUsageCounter { UserId = _userId, EvaluationCount = 3 };
        _subRepoMock.Setup(r => r.GetCounterAsync(_userId, It.IsAny<int>(), It.IsAny<int>(), default)).ReturnsAsync(counter);

        await _sut.IncrementEvaluationCountAsync(_userId, default);

        Assert.Equal(4, counter.EvaluationCount);
        _subRepoMock.Verify(r => r.UpdateCounter(counter), Times.Once);
    }

    [Fact]
    public async Task IncrementEvaluationCountAsync_SavesChanges()
    {
        var counter = new MonthlyUsageCounter { UserId = _userId, EvaluationCount = 0 };
        _subRepoMock.Setup(r => r.GetCounterAsync(_userId, It.IsAny<int>(), It.IsAny<int>(), default)).ReturnsAsync(counter);

        await _sut.IncrementEvaluationCountAsync(_userId, default);

        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
