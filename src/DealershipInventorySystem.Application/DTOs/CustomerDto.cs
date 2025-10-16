namespace DealershipInventorySystem.Application.DTOs;

public record CustomerDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string? CpfCnpj { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public int PurchasedVehiclesCount { get; init; }
}

public record CreateCustomerDto
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string? CpfCnpj { get; init; }
}

public record UpdateCustomerDto
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string? CpfCnpj { get; init; }
    public bool IsActive { get; init; }
}