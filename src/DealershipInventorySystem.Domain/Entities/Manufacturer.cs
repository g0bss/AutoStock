namespace DealershipInventorySystem.Domain.Entities;

public class Manufacturer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Ford, GM, Volkswagen, etc.
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relacionamentos
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}