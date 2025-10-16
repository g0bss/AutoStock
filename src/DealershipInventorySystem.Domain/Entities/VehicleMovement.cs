using DealershipInventorySystem.Domain.Enums;

namespace DealershipInventorySystem.Domain.Entities;

public class VehicleMovement
{
    public int Id { get; set; }
    public MovementType Type { get; set; } // Entrada, Test Drive, Reserva, Venda, Manutenção
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public decimal? Value { get; set; } // Valor da transação se aplicável
    public string? Notes { get; set; }

    // Relacionamentos
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}