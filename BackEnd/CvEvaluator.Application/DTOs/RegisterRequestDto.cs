using System.ComponentModel.DataAnnotations;

namespace CvEvaluator.Application.DTOs;

public class RegisterRequestDto
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public required string Email { get; set; }

    [Required]
    [MinLength(6)]
    [MaxLength(128)]
    public required string Password { get; set; }

    [Required]
    [MaxLength(200)]
    public required string FullName { get; set; }
}
