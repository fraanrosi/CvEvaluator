using CvEvaluator.Application.DTOs;

namespace CvEvaluator.Application.Interfaces;
public interface IIdentityService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
}