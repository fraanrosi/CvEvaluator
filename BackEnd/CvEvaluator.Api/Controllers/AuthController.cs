using CvEvaluator.Application.DTOs;
using CvEvaluator.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CvEvaluator.Api.Controllers;

[ApiController]
[Route("api/auth")]
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
        var response = await _identityService.RegisterAsync(request);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        var response = await _identityService.LoginAsync(request);
        return Ok(response);
    }
}