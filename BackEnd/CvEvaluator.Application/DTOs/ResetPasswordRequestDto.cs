using System.ComponentModel.DataAnnotations;

namespace CvEvaluator.Application.DTOs;

public class ResetPasswordRequestDto
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = default!;

    [Required]
    public string Token { get; set; } = default!;

    [Required]
    [MinLength(6)]
    [MaxLength(128)]
    public string NewPassword { get; set; } = default!;
}
