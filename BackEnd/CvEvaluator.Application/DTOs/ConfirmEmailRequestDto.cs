using System.ComponentModel.DataAnnotations;

namespace CvEvaluator.Application.DTOs;

public class ConfirmEmailRequestDto
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = default!;

    [Required]
    public string Token { get; set; } = default!;
}
