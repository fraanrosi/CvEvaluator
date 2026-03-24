using CvEvaluator.Api.Controllers;
using CvEvaluator.Application.DTOs;
using CvEvaluator.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace CvEvaluator.Api.Tests;

public class JobPositionsControllerTests
{
    private readonly Mock<IJobPositionService> _serviceMock;
    private readonly JobPositionsController _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public JobPositionsControllerTests()
    {
        _serviceMock = new Mock<IJobPositionService>();
        _sut = new JobPositionsController(_serviceMock.Object);
        _sut.ControllerContext = CreateContextWithUser(_userId);
    }

    private static ControllerContext CreateContextWithUser(Guid userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "test"));
        return new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetByIdAsync(id, _userId, default))
            .ReturnsAsync((JobPositionDetailDto?)null);

        var result = await _sut.GetById(id, default);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_Valid_ReturnsCreatedAtAction()
    {
        var dto = new CreateJobPositionDto { Title = "Dev", Description = "Desc" };
        var returned = new JobPositionDto { Id = Guid.NewGuid(), Title = "Dev", Description = "Desc", EvaluationsCount = 0 };
        _serviceMock.Setup(s => s.CreateAsync(dto, _userId, default)).ReturnsAsync(returned);

        var result = await _sut.Create(dto, default);

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task Update_NotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var dto = new UpdateJobPositionDto { Title = "New", Description = "New" };
        _serviceMock.Setup(s => s.UpdateAsync(id, dto, _userId, default)).ReturnsAsync(false);

        var result = await _sut.Update(id, dto, default);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_NotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteAsync(id, _userId, default)).ReturnsAsync(false);

        var result = await _sut.Delete(id, default);

        Assert.IsType<NotFoundResult>(result);
    }
}
