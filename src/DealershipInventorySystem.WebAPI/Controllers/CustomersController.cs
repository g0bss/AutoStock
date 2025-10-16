using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DealershipInventorySystem.Application.DTOs;
using DealershipInventorySystem.Domain.Entities;
using DealershipInventorySystem.Infrastructure.Data;

namespace DealershipInventorySystem.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly DealershipDbContext _context;

    public CustomersController(DealershipDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém todos os clientes ativos
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
    {
        var customers = await _context.Customers
            .Where(c => c.IsActive)
            .Include(c => c.PurchasedVehicles)
            .Select(c => MapToCustomerDto(c))
            .ToListAsync();

        return Ok(customers);
    }

    /// <summary>
    /// Obtém um cliente específico por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
    {
        var customer = await _context.Customers
            .Include(c => c.PurchasedVehicles)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
        {
            return NotFound("Cliente não encontrado");
        }

        return Ok(MapToCustomerDto(customer));
    }

    /// <summary>
    /// Busca clientes por CPF/CNPJ
    /// </summary>
    [HttpGet("search/cpf-cnpj/{cpfCnpj}")]
    public async Task<ActionResult<CustomerDto>> GetCustomerByCpfCnpj(string cpfCnpj)
    {
        var customer = await _context.Customers
            .Include(c => c.PurchasedVehicles)
            .FirstOrDefaultAsync(c => c.CpfCnpj == cpfCnpj && c.IsActive);

        if (customer == null)
        {
            return NotFound("Cliente não encontrado");
        }

        return Ok(MapToCustomerDto(customer));
    }

    /// <summary>
    /// Busca clientes por nome
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> SearchCustomers([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Nome para busca é obrigatório");
        }

        var customers = await _context.Customers
            .Where(c => c.IsActive && c.Name.ToLower().Contains(name.ToLower()))
            .Include(c => c.PurchasedVehicles)
            .Select(c => MapToCustomerDto(c))
            .ToListAsync();

        return Ok(customers);
    }

    /// <summary>
    /// Obtém veículos comprados por um cliente
    /// </summary>
    [HttpGet("{id}/vehicles")]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetCustomerVehicles(int id)
    {
        var customer = await _context.Customers
            .Include(c => c.PurchasedVehicles)
                .ThenInclude(v => v.Manufacturer)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
        {
            return NotFound("Cliente não encontrado");
        }

        var vehicles = customer.PurchasedVehicles.Select(v => new VehicleDto
        {
            Id = v.Id,
            Vin = v.Vin,
            Make = v.Make,
            Model = v.Model,
            Year = v.Year,
            Color = v.Color,
            Type = v.Type,
            Status = v.Status,
            FuelType = v.FuelType,
            TransmissionType = v.TransmissionType,
            Mileage = v.Mileage,
            CostPrice = v.CostPrice,
            SellingPrice = v.SellingPrice,
            ArrivalDate = v.ArrivalDate,
            SoldDate = v.SoldDate,
            Notes = v.Notes,
            ManufacturerId = v.ManufacturerId,
            ManufacturerName = v.Manufacturer?.Name ?? "",
            CustomerId = v.CustomerId,
            CustomerName = v.Customer?.Name
        });

        return Ok(vehicles);
    }

    /// <summary>
    /// Cria um novo cliente
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator,Manager,Salesperson")]
    public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerDto createCustomerDto)
    {
        // Verificar se já existe cliente com mesmo CPF/CNPJ
        if (!string.IsNullOrEmpty(createCustomerDto.CpfCnpj))
        {
            if (await _context.Customers.AnyAsync(c => c.CpfCnpj == createCustomerDto.CpfCnpj))
            {
                return BadRequest("Já existe um cliente com este CPF/CNPJ");
            }
        }

        // Verificar se já existe cliente com mesmo email
        if (!string.IsNullOrEmpty(createCustomerDto.Email))
        {
            if (await _context.Customers.AnyAsync(c => c.Email.ToLower() == createCustomerDto.Email.ToLower()))
            {
                return BadRequest("Já existe um cliente com este email");
            }
        }

        var customer = new Customer
        {
            Name = createCustomerDto.Name,
            Email = createCustomerDto.Email,
            Phone = createCustomerDto.Phone,
            Address = createCustomerDto.Address,
            CpfCnpj = createCustomerDto.CpfCnpj,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, MapToCustomerDto(customer));
    }

    /// <summary>
    /// Atualiza um cliente existente
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator,Manager,Salesperson")]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerDto updateCustomerDto)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
        {
            return NotFound("Cliente não encontrado");
        }

        // Verificar se outro cliente já tem esse CPF/CNPJ
        if (!string.IsNullOrEmpty(updateCustomerDto.CpfCnpj))
        {
            if (await _context.Customers.AnyAsync(c => c.CpfCnpj == updateCustomerDto.CpfCnpj && c.Id != id))
            {
                return BadRequest("Já existe outro cliente com este CPF/CNPJ");
            }
        }

        // Verificar se outro cliente já tem esse email
        if (!string.IsNullOrEmpty(updateCustomerDto.Email))
        {
            if (await _context.Customers.AnyAsync(c => c.Email.ToLower() == updateCustomerDto.Email.ToLower() && c.Id != id))
            {
                return BadRequest("Já existe outro cliente com este email");
            }
        }

        // Atualizar propriedades
        customer.Name = updateCustomerDto.Name;
        customer.Email = updateCustomerDto.Email;
        customer.Phone = updateCustomerDto.Phone;
        customer.Address = updateCustomerDto.Address;
        customer.CpfCnpj = updateCustomerDto.CpfCnpj;
        customer.IsActive = updateCustomerDto.IsActive;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Remove um cliente (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var customer = await _context.Customers
            .Include(c => c.PurchasedVehicles)
            .Include(c => c.Movements)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
        {
            return NotFound("Cliente não encontrado");
        }

        // Verificar se o cliente tem veículos ou movimentações
        if (customer.PurchasedVehicles.Any() || customer.Movements.Any())
        {
            return BadRequest("Não é possível excluir cliente com veículos comprados ou movimentações associadas. Desative o cliente em vez de excluí-lo.");
        }

        // Soft delete - marcar como inativo
        customer.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Desativa um cliente
    /// </summary>
    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<IActionResult> DeactivateCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
        {
            return NotFound("Cliente não encontrado");
        }

        customer.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Reativa um cliente
    /// </summary>
    [HttpPatch("{id}/activate")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<IActionResult> ActivateCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
        {
            return NotFound("Cliente não encontrado");
        }

        customer.IsActive = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Obtém relatório de compras do cliente
    /// </summary>
    [HttpGet("{id}/purchase-summary")]
    [Authorize(Roles = "Administrator,Manager,Salesperson")]
    public async Task<ActionResult<object>> GetCustomerPurchaseSummary(int id)
    {
        var customer = await _context.Customers
            .Include(c => c.PurchasedVehicles)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
        {
            return NotFound("Cliente não encontrado");
        }

        var summary = new
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            TotalVehiclesPurchased = customer.PurchasedVehicles.Count,
            TotalAmountSpent = customer.PurchasedVehicles.Sum(v => v.SellingPrice),
            FirstPurchaseDate = customer.PurchasedVehicles.Min(v => v.SoldDate),
            LastPurchaseDate = customer.PurchasedVehicles.Max(v => v.SoldDate),
            AverageVehiclePrice = customer.PurchasedVehicles.Any() ? customer.PurchasedVehicles.Average(v => v.SellingPrice) : 0,
            VehiclesByYear = customer.PurchasedVehicles
                .GroupBy(v => v.Year)
                .Select(g => new { Year = g.Key, Count = g.Count() })
                .OrderBy(x => x.Year)
        };

        return Ok(summary);
    }

    private static CustomerDto MapToCustomerDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            CpfCnpj = customer.CpfCnpj,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt,
            PurchasedVehiclesCount = customer.PurchasedVehicles.Count
        };
    }
}