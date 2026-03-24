using CvEvaluator.Api.Controllers;
using CvEvaluator.Application.DTOs;
using CvEvaluator.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace CvEvaluator.Api.Tests;

public class CvEvaluationControllerTests
{
    private readonly Mock<ICvEvaluationService> _serviceMock;
    private readonly Mock<ILogger<CvEvaluationController>> _loggerMock;
    private readonly CvEvaluationController _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public CvEvaluationControllerTests()
    {
        _serviceMock = new Mock<ICvEvaluationService>();
        _loggerMock = new Mock<ILogger<CvEvaluationController>>();
        _sut = new CvEvaluationController(_serviceMock.Object, _loggerMock.Object);
        _sut.ControllerContext = CreateContextWithUser(_userId);
    }

    private static ControllerContext CreateContextWithUser(Guid userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, "test@test.com")
        }, "test"));
        return new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
    }

    private static Mock<IFormFile> CreateMockFile(string name = "cv.pdf", long size = 1024)
    {
        var mock = new Mock<IFormFile>();
        mock.Setup(f => f.FileName).Returns(name);
        mock.Setup(f => f.Length).Returns(size);
        return mock;
    }

    [Fact]
    public async Task EvaluatePdf_NoFiles_ReturnsBadRequest()
    {
        var result = await _sut.EvaluatePdf(new List<IFormFile>(), Guid.NewGuid(), default);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task EvaluatePdf_EmptyFile_ReturnsErrorInResponseList()
    {
        var file = CreateMockFile(size: 0);
        var result = await _sut.EvaluatePdf(new List<IFormFile> { file.Object }, Guid.NewGuid(), default);

        var accepted = Assert.IsType<AcceptedResult>(result);
        var responses = Assert.IsAssignableFrom<IEnumerable<object>>(accepted.Value);
        Assert.Contains(responses, r => r.ToString()!.Contains("Empty file"));
    }

    [Fact]
    public async Task EvaluatePdf_FileTooBig_ReturnsErrorInResponseList()
    {
        var file = CreateMockFile(size: 11 * 1024 * 1024); // 11 MB
        var result = await _sut.EvaluatePdf(new List<IFormFile> { file.Object }, Guid.NewGuid(), default);

        var accepted = Assert.IsType<AcceptedResult>(result);
        var responses = Assert.IsAssignableFrom<IEnumerable<object>>(accepted.Value);
        Assert.Contains(responses, r => r.ToString()!.Contains("10 MB"));
    }

    [Fact]
    public async Task EvaluatePdf_NonPdfExtension_ReturnsErrorInResponseList()
    {
        var file = CreateMockFile(name: "cv.docx");
        var result = await _sut.EvaluatePdf(new List<IFormFile> { file.Object }, Guid.NewGuid(), default);

        var accepted = Assert.IsType<AcceptedResult>(result);
        var responses = Assert.IsAssignableFrom<IEnumerable<object>>(accepted.Value);
        Assert.Contains(responses, r => r.ToString()!.Contains("PDF"));
    }

    [Fact]
    public async Task EvaluatePdf_ValidFile_ReturnsAccepted()
    {
        var file = CreateMockFile();
        var evalId = Guid.NewGuid();
        _serviceMock.Setup(s => s.EvaluateAsync(file.Object, _userId, It.IsAny<Guid>(), default))
            .ReturnsAsync(evalId);

        var result = await _sut.EvaluatePdf(new List<IFormFile> { file.Object }, Guid.NewGuid(), default);

        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetByIdAsync(id, _userId, default))
            .ReturnsAsync((CvEvaluationDto?)null);

        var result = await _sut.GetById(id, _serviceMock.Object, default);

        Assert.IsType<NotFoundResult>(result);
    }
}
