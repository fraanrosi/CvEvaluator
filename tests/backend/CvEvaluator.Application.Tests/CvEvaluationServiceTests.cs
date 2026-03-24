using CvEvaluator.Application.DTOs;
using CvEvaluator.Application.Interfaces;
using CvEvaluator.Application.Services;
using CvEvaluator.Domain.Entities;
using CvEvaluator.Domain.Enums;
using CvEvaluator.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace CvEvaluator.Application.Tests;

public class CvEvaluationServiceTests
{
    private readonly Mock<IDocumentParser> _parserMock;
    private readonly Mock<ICvEvaluationRepository> _repoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IEvaluationQueue> _queueMock;
    private readonly Mock<IJobPositionRepository> _jobRepoMock;
    private readonly Mock<ISubscriptionService> _subServiceMock;
    private readonly CvEvaluationService _sut;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _jobPositionId = Guid.NewGuid();

    public CvEvaluationServiceTests()
    {
        _parserMock = new Mock<IDocumentParser>();
        _repoMock = new Mock<ICvEvaluationRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _queueMock = new Mock<IEvaluationQueue>();
        _jobRepoMock = new Mock<IJobPositionRepository>();
        _subServiceMock = new Mock<ISubscriptionService>();
        _sut = new CvEvaluationService(
            _parserMock.Object, _repoMock.Object, _uowMock.Object,
            _queueMock.Object, _jobRepoMock.Object, _subServiceMock.Object);
    }

    private Mock<IFormFile> CreateMockFormFile(string name = "cv.pdf", long length = 1024, string content = "PDF content")
    {
        var fileMock = new Mock<IFormFile>();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        fileMock.Setup(f => f.FileName).Returns(name);
        fileMock.Setup(f => f.Length).Returns(length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        return fileMock;
    }

    private void SetupValidJobPosition()
    {
        var jobPosition = new JobPosition { Id = _jobPositionId, UserId = _userId, Title = "Dev", Description = "Desc" };
        _jobRepoMock.Setup(r => r.GetByIdAsync(_jobPositionId, default)).ReturnsAsync(jobPosition);
    }

    // ─── EvaluateAsync ──────────────────────────────────────────────

    [Fact]
    public async Task EvaluateAsync_NullFile_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.EvaluateAsync(null!, _userId, _jobPositionId, default));
    }

    [Fact]
    public async Task EvaluateAsync_EmptyFile_ThrowsArgumentException()
    {
        var file = CreateMockFormFile(length: 0);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.EvaluateAsync(file.Object, _userId, _jobPositionId, default));
    }

    [Fact]
    public async Task EvaluateAsync_LimitExceeded_ThrowsPlanLimitExceededException()
    {
        var file = CreateMockFormFile();
        _subServiceMock.Setup(s => s.CheckEvaluationLimitAsync(_userId, default))
            .ThrowsAsync(new PlanLimitExceededException("evaluations", "Limit reached"));

        await Assert.ThrowsAsync<PlanLimitExceededException>(() =>
            _sut.EvaluateAsync(file.Object, _userId, _jobPositionId, default));
    }

    [Fact]
    public async Task EvaluateAsync_JobPositionNotFound_ThrowsInvalidOperationException()
    {
        var file = CreateMockFormFile();
        _jobRepoMock.Setup(r => r.GetByIdAsync(_jobPositionId, default)).ReturnsAsync((JobPosition?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.EvaluateAsync(file.Object, _userId, _jobPositionId, default));
    }

    [Fact]
    public async Task EvaluateAsync_JobPositionBelongsToDifferentUser_ThrowsInvalidOperationException()
    {
        var file = CreateMockFormFile();
        var otherUserId = Guid.NewGuid();
        var jobPosition = new JobPosition { Id = _jobPositionId, UserId = otherUserId, Title = "Dev", Description = "Desc" };
        _jobRepoMock.Setup(r => r.GetByIdAsync(_jobPositionId, default)).ReturnsAsync(jobPosition);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.EvaluateAsync(file.Object, _userId, _jobPositionId, default));
    }

    [Fact]
    public async Task EvaluateAsync_ExtractedTextEmpty_ThrowsInvalidOperationException()
    {
        var file = CreateMockFormFile();
        SetupValidJobPosition();
        _parserMock.Setup(p => p.ParseAsync(It.IsAny<Stream>())).ReturnsAsync(string.Empty);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.EvaluateAsync(file.Object, _userId, _jobPositionId, default));
    }

    [Fact]
    public async Task EvaluateAsync_ValidFile_CreatesEvaluationWithStatusProcessing()
    {
        var file = CreateMockFormFile();
        SetupValidJobPosition();
        _parserMock.Setup(p => p.ParseAsync(It.IsAny<Stream>())).ReturnsAsync("Extracted CV text");

        CvEvaluation? captured = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<CvEvaluation>(), default))
            .Callback<CvEvaluation, CancellationToken>((e, _) => captured = e);

        await _sut.EvaluateAsync(file.Object, _userId, _jobPositionId, default);

        Assert.NotNull(captured);
        Assert.Equal(EvaluationStatus.Processing, captured!.Status);
        Assert.Equal(_userId, captured.UserId);
        Assert.Equal(_jobPositionId, captured.JobPositionId);
        Assert.Equal("cv.pdf", captured.OriginalFilename);
    }

    [Fact]
    public async Task EvaluateAsync_ValidFile_ComputesSHA256Hash()
    {
        var file = CreateMockFormFile();
        SetupValidJobPosition();
        _parserMock.Setup(p => p.ParseAsync(It.IsAny<Stream>())).ReturnsAsync("Extracted CV text");

        CvEvaluation? captured = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<CvEvaluation>(), default))
            .Callback<CvEvaluation, CancellationToken>((e, _) => captured = e);

        await _sut.EvaluateAsync(file.Object, _userId, _jobPositionId, default);

        Assert.NotNull(captured);
        Assert.NotNull(captured!.FileHash);
        Assert.NotEmpty(captured.FileHash);
        Assert.Equal(64, captured.FileHash.Length); // SHA256 hex = 64 chars
    }

    [Fact]
    public async Task EvaluateAsync_ValidFile_CallsSaveEnqueueAndIncrementInOrder()
    {
        var file = CreateMockFormFile();
        SetupValidJobPosition();
        _parserMock.Setup(p => p.ParseAsync(It.IsAny<Stream>())).ReturnsAsync("Extracted CV text");

        await _sut.EvaluateAsync(file.Object, _userId, _jobPositionId, default);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<CvEvaluation>(), default), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        _subServiceMock.Verify(s => s.IncrementEvaluationCountAsync(_userId, default), Times.Once);
        _queueMock.Verify(q => q.EnqueueAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateAsync_ValidFile_ReturnsNewGuid()
    {
        var file = CreateMockFormFile();
        SetupValidJobPosition();
        _parserMock.Setup(p => p.ParseAsync(It.IsAny<Stream>())).ReturnsAsync("Extracted CV text");

        var result = await _sut.EvaluateAsync(file.Object, _userId, _jobPositionId, default);

        Assert.NotEqual(Guid.Empty, result);
    }

    // ─── GetByIdAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((CvEvaluation?)null);

        var result = await _sut.GetByIdAsync(id, _userId, default);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_DifferentUser_ReturnsNull()
    {
        var id = Guid.NewGuid();
        var eval = new CvEvaluation { Id = id, UserId = Guid.NewGuid(), OriginalFilename = "cv.pdf", ExtractedText = "text", FileHash = "hash" };
        _repoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync(eval);

        var result = await _sut.GetByIdAsync(id, _userId, default);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ValidOwner_ReturnsMappedDto()
    {
        var id = Guid.NewGuid();
        var eval = new CvEvaluation
        {
            Id = id, UserId = _userId, JobPositionId = _jobPositionId,
            OriginalFilename = "cv.pdf", Status = EvaluationStatus.Completed,
            OverallScore = 85m, ExtractedText = "text", FileHash = "hash"
        };
        _repoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync(eval);

        var result = await _sut.GetByIdAsync(id, _userId, default);

        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.Equal("cv.pdf", result.OriginalFilename);
        Assert.Equal(85m, result.OverallScore);
    }

    [Fact]
    public async Task GetByIdAsync_WithJsonResult_DeserializesFieldsCorrectly()
    {
        var id = Guid.NewGuid();
        var json = """{"score":90,"yearsExperience":5,"matchesRequirements":true,"strengths":["C#"],"weaknesses":["SQL"]}""";
        var eval = new CvEvaluation
        {
            Id = id, UserId = _userId, JobPositionId = _jobPositionId,
            OriginalFilename = "cv.pdf", Status = EvaluationStatus.Completed,
            EvaluationResult = json, ExtractedText = "text", FileHash = "hash"
        };
        _repoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync(eval);

        var result = await _sut.GetByIdAsync(id, _userId, default);

        Assert.NotNull(result);
        Assert.Equal(5, result!.YearsExperience);
        Assert.True(result.MatchesRequirements);
        Assert.Contains("C#", result.Strengths!);
        Assert.Contains("SQL", result.Weaknesses!);
    }

    [Fact]
    public async Task GetByIdAsync_WithMalformedJson_ReturnsNullResultFields()
    {
        var id = Guid.NewGuid();
        var eval = new CvEvaluation
        {
            Id = id, UserId = _userId, JobPositionId = _jobPositionId,
            OriginalFilename = "cv.pdf", EvaluationResult = "not valid json{{{",
            ExtractedText = "text", FileHash = "hash"
        };
        _repoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync(eval);

        var result = await _sut.GetByIdAsync(id, _userId, default);

        Assert.NotNull(result);
        Assert.Null(result!.Strengths);
        Assert.Null(result.Weaknesses);
        Assert.Null(result.YearsExperience);
    }

    // ─── GetAllByUserAsync ──────────────────────────────────────────

    [Fact]
    public async Task GetAllByUserAsync_ReturnsMappedDtos()
    {
        var evaluations = new List<CvEvaluation>
        {
            new() { Id = Guid.NewGuid(), UserId = _userId, OriginalFilename = "a.pdf", Status = EvaluationStatus.Completed, ExtractedText = "text", FileHash = "hash" },
            new() { Id = Guid.NewGuid(), UserId = _userId, OriginalFilename = "b.pdf", Status = EvaluationStatus.Processing, ExtractedText = "text", FileHash = "hash" }
        };
        _repoMock.Setup(r => r.GetAllByUserIdAsync(_userId, default)).ReturnsAsync(evaluations);

        var result = (await _sut.GetAllByUserAsync(_userId, default)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("a.pdf", result[0].OriginalFilename);
        Assert.Equal("b.pdf", result[1].OriginalFilename);
    }
}
