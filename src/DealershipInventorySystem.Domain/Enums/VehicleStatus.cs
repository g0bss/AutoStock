namespace DealershipInventorySystem.Domain.Enums;

public enum VehicleStatus
{
    Available = 1,      // Disponível para venda
    Reserved = 2,       // Reservado por um cliente
    TestDrive = 3,      // Em test drive
    Maintenance = 4,    // Em manutenção
    Sold = 5,          // Vendido
    Inactive = 6       // Inativo (não disponível)
}