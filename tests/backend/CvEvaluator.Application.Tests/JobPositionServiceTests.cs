using CvEvaluator.Application.DTOs;
using CvEvaluator.Application.Interfaces;
using CvEvaluator.Application.Services;
using CvEvaluator.Domain.Entities;
using CvEvaluator.Domain.Enums;
using CvEvaluator.Domain.Exceptions;
using Moq;

namespace CvEvaluator.Application.Tests;

public class JobPositionServiceTests
{
    private readonly Mock<IJobPositionRepository> _repoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ISubscriptionService> _subServiceMock;
    private readonly JobPositionService _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public JobPositionServiceTests()
    {
        _repoMock = new Mock<IJobPositionRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _subServiceMock = new Mock<ISubscriptionService>();
        _sut = new JobPositionService(_repoMock.Object, _uowMock.Object, _subServiceMock.Object);
    }

    private JobPosition CreatePosition(Guid? id = null, Guid? userId = null, int evalCount = 0)
    {
        var pos = new JobPosition
        {
            Id = id ?? Guid.NewGuid(),
            UserId = userId ?? _userId,
            Title = "Backend Developer",
            Description = "C# and .NET experience required",
            CreatedAt = DateTime.UtcNow
        };
        for (var i = 0; i < evalCount; i++)
            pos.Evaluations.Add(new CvEvaluation
            {
                Id = Guid.NewGuid(), UserId = pos.UserId, JobPositionId = pos.Id,
                OriginalFilename = $"cv_{i}.pdf", Status = EvaluationStatus.Completed,
                OverallScore = 70 + i, ExtractedText = "text", FileHash = "hash",
                EvaluatedAt = DateTime.UtcNow
            });
        return pos;
    }

    // ─── GetAllAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllPositionsMappedToDtos()
    {
        var positions = new List<JobPosition> { CreatePosition(evalCount: 2), CreatePosition(evalCount: 0) };
        _repoMock.Setup(r => r.GetByUserIdAsync(_userId, default)).ReturnsAsync(positions);

        var result = (await _sut.GetAllAsync(_userId, default)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Backend Developer", result[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_EmptyList_ReturnsEmpty()
    {
        _repoMock.Setup(r => r.GetByUserIdAsync(_userId, default)).ReturnsAsync(new List<JobPosition>());

        var result = await _sut.GetAllAsync(_userId, default);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_MapsEvaluationsCountCorrectly()
    {
        var positions = new List<JobPosition> { CreatePosition(evalCount: 3) };
        _repoMock.Setup(r => r.GetByUserIdAsync(_userId, default)).ReturnsAsync(positions);

        var result = (await _sut.GetAllAsync(_userId, default)).ToList();

        Assert.Equal(3, result[0].EvaluationsCount);
    }

    // ─── GetByIdAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdWithEvaluationsAsync(id, default)).ReturnsAsync((JobPosition?)null);

        var result = await _sut.GetByIdAsync(id, _userId, default);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_DifferentUser_ReturnsNull()
    {
        var id = Guid.NewGuid();
        var position = CreatePosition(id: id, userId: Guid.NewGuid());
        _repoMock.Setup(r => r.GetByIdWithEvaluationsAsync(id, default)).ReturnsAsync(position);

        var result = await _sut.GetByIdAsync(id, _userId, default);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ValidOwner_ReturnsMappedDetailDto()
    {
        var id = Guid.NewGuid();
        var position = CreatePosition(id: id, evalCount: 1);
        _repoMock.Setup(r => r.GetByIdWithEvaluationsAsync(id, default)).ReturnsAsync(position);

        var result = await _sut.GetByIdAsync(id, _userId, default);

        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.Equal("Backend Developer", result.Title);
        Assert.Single(result.Evaluations);
    }

    [Fact]
    public async Task GetByIdAsync_ExtractsCandidateNameFromFilename()
    {
        var id = Guid.NewGuid();
        var position = CreatePosition(id: id, evalCount: 1);
        position.Evaluations.First().OriginalFilename = "John_Doe_CV.pdf";
        _repoMock.Setup(r => r.GetByIdWithEvaluationsAsync(id, default)).ReturnsAsync(position);

        var result = await _sut.GetByIdAsync(id, _userId, default);

        Assert.Equal("John_Doe_CV", result!.Evaluations[0].CandidateName);
    }

    // ─── CreateAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ChecksJobPositionLimit()
    {
        var dto = new CreateJobPositionDto { Title = "Dev", Description = "Desc" };

        await _sut.CreateAsync(dto, _userId, default);

        _subServiceMock.Verify(s => s.CheckJobPositionLimitAsync(_userId, default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_LimitExceeded_ThrowsPlanLimitExceededException()
    {
        var dto = new CreateJobPositionDto { Title = "Dev", Description = "Desc" };
        _subServiceMock.Setup(s => s.CheckJobPositionLimitAsync(_userId, default))
            .ThrowsAsync(new PlanLimitExceededException("jobPositions", "Limit reached"));

        await Assert.ThrowsAsync<PlanLimitExceededException>(() =>
            _sut.CreateAsync(dto, _userId, default));
    }

    [Fact]
    public async Task CreateAsync_ValidDto_CreatesWithCorrectUserIdAndSaves()
    {
        var dto = new CreateJobPositionDto { Title = "Frontend Dev", Description = "React experience" };

        JobPosition? captured = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<JobPosition>(), default))
            .Callback<JobPosition, CancellationToken>((p, _) => captured = p);

        await _sut.CreateAsync(dto, _userId, default);

        Assert.NotNull(captured);
        Assert.Equal(_userId, captured!.UserId);
        Assert.Equal("Frontend Dev", captured.Title);
        Assert.Equal("React experience", captured.Description);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNewDtoWithZeroEvaluations()
    {
        var dto = new CreateJobPositionDto { Title = "Dev", Description = "Desc" };

        var result = await _sut.CreateAsync(dto, _userId, default);

        Assert.Equal("Dev", result.Title);
        Assert.Equal(0, result.EvaluationsCount);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    // ─── UpdateAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_NotFoundOrWrongUser_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        var dto = new UpdateJobPositionDto { Title = "New", Description = "New" };
        _repoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((JobPosition?)null);

        var result = await _sut.UpdateAsync(id, dto, _userId, default);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_ValidOwner_UpdatesFieldsAndReturnsTrue()
    {
        var id = Guid.NewGuid();
        var position = CreatePosition(id: id);
        var dto = new UpdateJobPositionDto { Title = "Updated Title", Description = "Updated Desc" };
        _repoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync(position);

        var result = await _sut.UpdateAsync(id, dto, _userId, default);

        Assert.True(result);
        Assert.Equal("Updated Title", position.Title);
        Assert.Equal("Updated Desc", position.Description);
        _repoMock.Verify(r => r.Update(position), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    // ─── DeleteAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_NotFoundOrWrongUser_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((JobPosition?)null);

        var result = await _sut.DeleteAsync(id, _userId, default);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_ValidOwner_DeletesAndReturnsTrue()
    {
        var id = Guid.NewGuid();
        var position = CreatePosition(id: id);
        _repoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync(position);

        var result = await _sut.DeleteAsync(id, _userId, default);

        Assert.True(result);
        _repoMock.Verify(r => r.Delete(position), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
