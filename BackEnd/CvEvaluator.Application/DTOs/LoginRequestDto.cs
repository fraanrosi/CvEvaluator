using System.ComponentModel.DataAnnotations;

namespace CvEvaluator.Application.DTOs;

public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public required string Email { get; set; }

    [Required]
    [MinLength(6)]
    [MaxLength(128)]
    public required string Password { get; set; }
}
