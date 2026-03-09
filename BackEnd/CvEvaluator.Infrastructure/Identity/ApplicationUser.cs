namespace CvEvaluator.Infrastructure.Identity;

using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    // opcional
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
