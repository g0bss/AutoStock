namespace DealershipInventorySystem.Domain.Enums;

public enum MovementType
{
    Entry = 1,          // Entrada de veículo no estoque
    TestDrive = 2,      // Test drive
    Reservation = 3,    // Reserva do veículo
    Sale = 4,           // Venda do veículo
    Maintenance = 5,    // Manutenção
    Transfer = 6,       // Transferência
    Inspection = 7,     // Vistoria
    StatusChange = 8    // Mudança de status
}