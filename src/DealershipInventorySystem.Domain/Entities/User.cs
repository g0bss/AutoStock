using DealershipInventorySystem.Domain.Enums;

namespace DealershipInventorySystem.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Operator;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Relacionamentos
    public ICollection<VehicleMovement> Movements { get; set; } = new List<VehicleMovement>();

    public string FullName => $"{FirstName} {LastName}";
}