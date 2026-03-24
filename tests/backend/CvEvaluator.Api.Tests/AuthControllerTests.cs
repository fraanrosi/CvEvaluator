using CvEvaluator.Api.Controllers;
using CvEvaluator.Application.DTOs;
using CvEvaluator.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CvEvaluator.Api.Tests;

public class AuthControllerTests
{
    private readonly Mock<IIdentityService> _identityMock;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _identityMock = new Mock<IIdentityService>();
        _sut = new AuthController(_identityMock.Object);
    }

    // ─── Register ───────────────────────────────────────────────────

    [Fact]
    public async Task Register_Success_ReturnsOkWithMessage()
    {
        _identityMock.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequestDto>()))
            .ReturnsAsync(new AuthResponseDto { Token = "", Email = "test@test.com" });

        var request = new RegisterRequestDto { Email = "test@test.com", Password = "Pass123!", FullName = "Test" };
        var result = await _sut.Register(request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Register_UserExists_ReturnsConflict409()
    {
        _identityMock.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequestDto>()))
            .ThrowsAsync(new Exception("User already exists."));

        var request = new RegisterRequestDto { Email = "test@test.com", Password = "Pass123!", FullName = "Test" };
        var result = await _sut.Register(request);

        Assert.IsType<ConflictObjectResult>(result);
    }

    // ─── Login ──────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        _identityMock.Setup(s => s.LoginAsync(It.IsAny<LoginRequestDto>()))
            .ReturnsAsync(new AuthResponseDto { Token = "jwt-token", Email = "test@test.com" });

        var request = new LoginRequestDto { Email = "test@test.com", Password = "Pass123!" };
        var result = await _sut.Login(request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized401()
    {
        _identityMock.Setup(s => s.LoginAsync(It.IsAny<LoginRequestDto>()))
            .ThrowsAsync(new Exception("Invalid credentials."));

        var request = new LoginRequestDto { Email = "test@test.com", Password = "Wrong!" };
        var result = await _sut.Login(request);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_EmailNotConfirmed_ReturnsBadRequestWithCode()
    {
        _identityMock.Setup(s => s.LoginAsync(It.IsAny<LoginRequestDto>()))
            .ThrowsAsync(new Exception("Email not confirmed. Please check your inbox."));

        var request = new LoginRequestDto { Email = "test@test.com", Password = "Pass123!" };
        var result = await _sut.Login(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ─── Other endpoints ────────────────────────────────────────────

    [Fact]
    public async Task ForgotPassword_Always_ReturnsOk()
    {
        var request = new ForgotPasswordRequestDto { Email = "anyone@test.com" };
        var result = await _sut.ForgotPassword(request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ConfirmEmail_InvalidToken_ReturnsBadRequest()
    {
        _identityMock.Setup(s => s.ConfirmEmailAsync(It.IsAny<ConfirmEmailRequestDto>()))
            .ThrowsAsync(new Exception("Invalid or expired confirmation token."));

        var request = new ConfirmEmailRequestDto { Email = "test@test.com", Token = "bad-token" };
        var result = await _sut.ConfirmEmail(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ResetPassword_Success_ReturnsOk()
    {
        var request = new ResetPasswordRequestDto { Email = "test@test.com", Token = "t", NewPassword = "New123!" };
        var result = await _sut.ResetPassword(request);

        Assert.IsType<OkObjectResult>(result);
    }
}
