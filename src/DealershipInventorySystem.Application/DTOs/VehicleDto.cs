using DealershipInventorySystem.Domain.Enums;

namespace DealershipInventorySystem.Application.DTOs;

public record VehicleDto
{
    public int Id { get; init; }
    public string Vin { get; init; } = string.Empty;
    public string Make { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int Year { get; init; }
    public string Color { get; init; } = string.Empty;
    public VehicleType Type { get; init; }
    public VehicleStatus Status { get; init; }
    public FuelType FuelType { get; init; }
    public TransmissionType TransmissionType { get; init; }
    public int Mileage { get; init; }
    public decimal CostPrice { get; init; }
    public decimal SellingPrice { get; init; }
    public DateTime ArrivalDate { get; init; }
    public DateTime? SoldDate { get; init; }
    public string? Notes { get; init; }
    public int ManufacturerId { get; init; }
    public string ManufacturerName { get; init; } = string.Empty;
    public int? CustomerId { get; init; }
    public string? CustomerName { get; init; }
}

public record CreateVehicleDto
{
    public string Vin { get; init; } = string.Empty;
    public string Make { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int Year { get; init; }
    public string Color { get; init; } = string.Empty;
    public VehicleType Type { get; init; }
    public VehicleStatus Status { get; init; }
    public FuelType FuelType { get; init; }
    public TransmissionType TransmissionType { get; init; }
    public int Mileage { get; init; }
    public decimal CostPrice { get; init; }
    public decimal SellingPrice { get; init; }
    public string? Notes { get; init; }
    public int ManufacturerId { get; init; }
}

public record UpdateVehicleDto
{
    public string Make { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int Year { get; init; }
    public string Color { get; init; } = string.Empty;
    public VehicleType Type { get; init; }
    public VehicleStatus Status { get; init; }
    public FuelType FuelType { get; init; }
    public TransmissionType TransmissionType { get; init; }
    public int Mileage { get; init; }
    public decimal CostPrice { get; init; }
    public decimal SellingPrice { get; init; }
    public string? Notes { get; init; }
    public int ManufacturerId { get; init; }
}