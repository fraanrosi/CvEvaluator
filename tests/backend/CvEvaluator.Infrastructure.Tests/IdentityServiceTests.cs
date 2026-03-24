using CvEvaluator.Application.DTOs;
using CvEvaluator.Application.Interfaces;
using CvEvaluator.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CvEvaluator.Infrastructure.Tests;

public class IdentityServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly IConfiguration _configuration;
    private readonly Mock<ISubscriptionService> _subServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly IdentityService _sut;

    public IdentityServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsATestSecretKeyThatIsLongEnoughForHS256Algorithm!",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Email:RequireConfirmation"] = "true",
                ["Email:FrontendUrl"] = "http://localhost:4200"
            })
            .Build();

        _subServiceMock = new Mock<ISubscriptionService>();
        _emailServiceMock = new Mock<IEmailService>();

        _sut = new IdentityService(
            _userManagerMock.Object, _configuration,
            _subServiceMock.Object, _emailServiceMock.Object);
    }

    private ApplicationUser CreateUser(string email = "test@test.com", bool emailConfirmed = true)
        => new() { Id = Guid.NewGuid(), Email = email, UserName = email, EmailConfirmed = emailConfirmed };

    // ─── RegisterAsync ──────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_UserAlreadyExists_ThrowsException()
    {
        var user = CreateUser();
        _userManagerMock.Setup(m => m.FindByEmailAsync("test@test.com")).ReturnsAsync(user);

        var request = new RegisterRequestDto { Email = "test@test.com", Password = "Pass123!", FullName = "Test" };

        var ex = await Assert.ThrowsAsync<Exception>(() => _sut.RegisterAsync(request));
        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task RegisterAsync_CreateFails_ThrowsExceptionWithErrors()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

        var request = new RegisterRequestDto { Email = "test@test.com", Password = "123", FullName = "Test" };

        var ex = await Assert.ThrowsAsync<Exception>(() => _sut.RegisterAsync(request));
        Assert.Contains("Password too weak", ex.Message);
    }

    [Fact]
    public async Task RegisterAsync_Success_AssignsFreePlan()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("token");

        var request = new RegisterRequestDto { Email = "test@test.com", Password = "Pass123!", FullName = "Test" };

        await _sut.RegisterAsync(request);

        _subServiceMock.Verify(s => s.AssignFreePlanAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ConfirmationRequired_SendsEmailConfirmation()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("confirm-token");

        var request = new RegisterRequestDto { Email = "test@test.com", Password = "Pass123!", FullName = "Test" };

        await _sut.RegisterAsync(request);

        _emailServiceMock.Verify(e => e.SendEmailConfirmationAsync("test@test.com", It.Is<string>(l => l.Contains("confirm-email")), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ConfirmationNotRequired_AutoConfirmsEmail()
    {
        // Override config to not require confirmation
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsATestSecretKeyThatIsLongEnoughForHS256Algorithm!",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Email:RequireConfirmation"] = "false"
            })
            .Build();

        var sut = new IdentityService(_userManagerMock.Object, config, _subServiceMock.Object, _emailServiceMock.Object);

        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("token");
        _userManagerMock.Setup(m => m.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var request = new RegisterRequestDto { Email = "test@test.com", Password = "Pass123!", FullName = "Test" };

        await sut.RegisterAsync(request);

        _userManagerMock.Verify(m => m.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), "token"), Times.Once);
        _emailServiceMock.Verify(e => e.SendEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsResponseWithEmptyToken()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("token");

        var request = new RegisterRequestDto { Email = "test@test.com", Password = "Pass123!", FullName = "Test" };

        var result = await _sut.RegisterAsync(request);

        Assert.Equal(string.Empty, result.Token);
        Assert.Equal("test@test.com", result.Email);
    }

    // ─── LoginAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsInvalidCredentials()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var request = new LoginRequestDto { Email = "nope@test.com", Password = "Pass123!" };

        var ex = await Assert.ThrowsAsync<Exception>(() => _sut.LoginAsync(request));
        Assert.Contains("Invalid credentials", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsInvalidCredentials()
    {
        var user = CreateUser();
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(false);

        var request = new LoginRequestDto { Email = "test@test.com", Password = "WrongPass!" };

        var ex = await Assert.ThrowsAsync<Exception>(() => _sut.LoginAsync(request));
        Assert.Contains("Invalid credentials", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_EmailNotConfirmedAndRequired_ThrowsException()
    {
        var user = CreateUser(emailConfirmed: false);
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(true);

        var request = new LoginRequestDto { Email = "test@test.com", Password = "Pass123!" };

        var ex = await Assert.ThrowsAsync<Exception>(() => _sut.LoginAsync(request));
        Assert.Contains("Email not confirmed", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsJwtToken()
    {
        var user = CreateUser();
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(true);

        var request = new LoginRequestDto { Email = "test@test.com", Password = "Pass123!" };

        var result = await _sut.LoginAsync(request);

        Assert.NotEmpty(result.Token);
        Assert.Equal("test@test.com", result.Email);
    }

    [Fact]
    public async Task LoginAsync_JwtToken_ContainsCorrectClaims()
    {
        var user = CreateUser();
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(true);

        var request = new LoginRequestDto { Email = "test@test.com", Password = "Pass123!" };

        var result = await _sut.LoginAsync(request);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Token);
        Assert.Equal(user.Id.ToString(), jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal("test@test.com", jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
    }

    [Fact]
    public async Task LoginAsync_JwtToken_ExpiresIn2Hours()
    {
        var user = CreateUser();
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(true);

        var result = await _sut.LoginAsync(new LoginRequestDto { Email = "test@test.com", Password = "Pass123!" });

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Token);
        var expiry = jwt.ValidTo;
        Assert.InRange(expiry, DateTime.UtcNow.AddHours(1.9), DateTime.UtcNow.AddHours(2.1));
    }

    // ─── ConfirmEmailAsync ──────────────────────────────────────────

    [Fact]
    public async Task ConfirmEmailAsync_UserNotFound_ThrowsException()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<Exception>(() =>
            _sut.ConfirmEmailAsync(new ConfirmEmailRequestDto { Email = "nope@test.com", Token = "token" }));
    }

    [Fact]
    public async Task ConfirmEmailAsync_InvalidToken_ThrowsException()
    {
        var user = CreateUser(emailConfirmed: false);
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.ConfirmEmailAsync(user, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _sut.ConfirmEmailAsync(new ConfirmEmailRequestDto { Email = "test@test.com", Token = "bad-token" }));
        Assert.Contains("Invalid or expired", ex.Message);
    }

    // ─── ForgotPasswordAsync ────────────────────────────────────────

    [Fact]
    public async Task ForgotPasswordAsync_UserNotFound_ReturnsSilently()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        await _sut.ForgotPasswordAsync(new ForgotPasswordRequestDto { Email = "nope@test.com" });

        _emailServiceMock.Verify(e => e.SendPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ForgotPasswordAsync_ValidUser_SendsResetEmail()
    {
        var user = CreateUser();
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");

        await _sut.ForgotPasswordAsync(new ForgotPasswordRequestDto { Email = "test@test.com" });

        _emailServiceMock.Verify(e => e.SendPasswordResetAsync("test@test.com", It.Is<string>(l => l.Contains("reset-password")), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ─── ResetPasswordAsync ─────────────────────────────────────────

    [Fact]
    public async Task ResetPasswordAsync_UserNotFound_ThrowsException()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<Exception>(() =>
            _sut.ResetPasswordAsync(new ResetPasswordRequestDto { Email = "nope@test.com", Token = "t", NewPassword = "New123!" }));
    }

    // ─── ResendConfirmationEmailAsync ───────────────────────────────

    [Fact]
    public async Task ResendConfirmationEmailAsync_AlreadyConfirmed_ReturnsSilently()
    {
        var user = CreateUser(emailConfirmed: true);
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

        await _sut.ResendConfirmationEmailAsync(new ForgotPasswordRequestDto { Email = "test@test.com" });

        _emailServiceMock.Verify(e => e.SendEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
