using System.ComponentModel.DataAnnotations;

namespace CvEvaluator.Application.DTOs;

public class CreateJobPositionDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = default!;

    [Required]
    [MaxLength(5000)]
    public string Description { get; set; } = default!;
}
