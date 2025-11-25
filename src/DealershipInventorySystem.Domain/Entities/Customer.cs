namespace DealershipInventorySystem.Domain.Entities;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? CpfCnpj { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Relacionamentos
    public ICollection<Vehicle> PurchasedVehicles { get; set; } = new List<Vehicle>();
    public ICollection<VehicleMovement> Movements { get; set; } = new List<VehicleMovement>();
}