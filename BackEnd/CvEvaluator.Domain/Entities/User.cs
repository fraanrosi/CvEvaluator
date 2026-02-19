using CvEvaluator.Domain.Enums;

namespace CvEvaluator.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }

    public string Email { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public UserRole Role { get; private set; }
    public ICollection<CvEvaluation> Evaluations { get; set; } = new List<CvEvaluation>();

    public DateTime CreatedAt { get; private set; }

    // EF Constructor
    private User() { }

    public User(string email, string name, string passwordHash, UserRole role = UserRole.User)
    {
        Id = Guid.NewGuid();
        Email = email;
        Name = name;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }

    public void ChangeRole(UserRole role)
    {
        Role = role;
    }
}
