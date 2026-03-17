using CvEvaluator.Application.DTOs;

namespace CvEvaluator.Application.Interfaces;
public interface IIdentityService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task ConfirmEmailAsync(ConfirmEmailRequestDto request);
    Task ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task ResetPasswordAsync(ResetPasswordRequestDto request);
    Task ResendConfirmationEmailAsync(ForgotPasswordRequestDto request);
}