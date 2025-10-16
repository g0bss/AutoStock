using DealershipInventorySystem.Domain.Enums;

namespace DealershipInventorySystem.Domain.Entities;

public class Vehicle
{
    public Vehicle()
    {
        ArrivalDate = DateTime.UtcNow;
    }

    public int Id { get; set; }
    public string Vin { get; set; } = string.Empty; // Chassis/VIN único
    public string Make { get; set; } = string.Empty; // Marca (Ford, GM, etc.)
    public string Model { get; set; } = string.Empty; // Modelo (Focus, Onix, etc.)
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public VehicleType Type { get; set; } = VehicleType.New;
    public VehicleStatus Status { get; set; } = VehicleStatus.Available;
    public FuelType FuelType { get; set; } = FuelType.Gasoline;
    public TransmissionType TransmissionType { get; set; } = TransmissionType.Manual;
    public int Mileage { get; set; } // Quilometragem
    public decimal CostPrice { get; set; } // Preço de custo
    public decimal SellingPrice { get; set; } // Preço de venda
    public DateTime ArrivalDate { get; set; }
    public DateTime? SoldDate { get; set; }
    public string? Notes { get; set; }

    // Relacionamentos
    public int ManufacturerId { get; set; }
    public Manufacturer Manufacturer { get; set; } = null!;

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public ICollection<VehicleMovement> Movements { get; set; } = new List<VehicleMovement>();
}