using System.ComponentModel.DataAnnotations;

namespace CvEvaluator.Application.DTOs;

public class ForgotPasswordRequestDto
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = default!;
}
