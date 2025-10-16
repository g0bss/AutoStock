using DealershipInventorySystem.Domain.Enums;

namespace DealershipInventorySystem.Application.DTOs;

public record VehicleMovementDto
{
    public int Id { get; init; }
    public MovementType Type { get; init; }
    public DateTime MovementDate { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal? Value { get; init; }
    public string? Notes { get; init; }
    public int VehicleId { get; init; }
    public string VehicleVin { get; init; } = string.Empty;
    public string VehicleMakeModel { get; init; } = string.Empty;
    public int? CustomerId { get; init; }
    public string? CustomerName { get; init; }
    public int UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
}

public record CreateVehicleMovementDto
{
    public MovementType Type { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal? Value { get; init; }
    public string? Notes { get; init; }
    public int VehicleId { get; init; }
    public int? CustomerId { get; init; }
}