using CvEvaluator.Application.DTOs;
using CvEvaluator.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CvEvaluator.Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public AuthController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto request)
    {
        try
        {
            var response = await _identityService.RegisterAsync(request);
            return Ok(new { message = "Registration successful. Please check your email to confirm your account.", email = response.Email });
        }
        catch (Exception ex) when (ex.Message == "User already exists.")
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        try
        {
            var response = await _identityService.LoginAsync(request);
            return Ok(response);
        }
        catch (Exception ex) when (ex.Message == "Email not confirmed. Please check your inbox.")
        {
            return BadRequest(new { error = ex.Message, code = "EMAIL_NOT_CONFIRMED" });
        }
        catch (Exception ex) when (ex.Message == "Invalid credentials.")
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailRequestDto request)
    {
        try
        {
            await _identityService.ConfirmEmailAsync(request);
            return Ok(new { message = "Email confirmed successfully. You can now log in." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto request)
    {
        await _identityService.ForgotPasswordAsync(request);
        return Ok(new { message = "If an account with that email exists, a password reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto request)
    {
        await _identityService.ResetPasswordAsync(request);
        return Ok(new { message = "Password has been reset successfully." });
    }

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation(ForgotPasswordRequestDto request)
    {
        await _identityService.ResendConfirmationEmailAsync(request);
        return Ok(new { message = "If an account with that email exists and is not yet confirmed, a confirmation link has been sent." });
    }
}
