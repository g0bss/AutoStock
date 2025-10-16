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
public class ManufacturersController : ControllerBase
{
    private readonly DealershipDbContext _context;

    public ManufacturersController(DealershipDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém todos os fabricantes ativos
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ManufacturerDto>>> GetManufacturers()
    {
        var manufacturers = await _context.Manufacturers
            .Where(m => m.IsActive)
            .Select(m => MapToManufacturerDto(m))
            .ToListAsync();

        return Ok(manufacturers);
    }

    /// <summary>
    /// Obtém um fabricante específico por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ManufacturerDto>> GetManufacturer(int id)
    {
        var manufacturer = await _context.Manufacturers
            .FirstOrDefaultAsync(m => m.Id == id);

        if (manufacturer == null)
        {
            return NotFound("Fabricante não encontrado");
        }

        return Ok(MapToManufacturerDto(manufacturer));
    }

    /// <summary>
    /// Obtém fabricantes com contagem de veículos
    /// </summary>
    [HttpGet("with-vehicle-count")]
    public async Task<ActionResult<IEnumerable<object>>> GetManufacturersWithVehicleCount()
    {
        var manufacturersWithCount = await _context.Manufacturers
            .Where(m => m.IsActive)
            .Select(m => new
            {
                m.Id,
                m.Name,
                m.ContactName,
                m.Email,
                m.Phone,
                m.Address,
                m.Country,
                m.IsActive,
                m.CreatedAt,
                VehicleCount = m.Vehicles.Count()
            })
            .ToListAsync();

        return Ok(manufacturersWithCount);
    }

    /// <summary>
    /// Cria um novo fabricante
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<ActionResult<ManufacturerDto>> CreateManufacturer([FromBody] CreateManufacturerDto createManufacturerDto)
    {
        // Verificar se fabricante com mesmo nome já existe
        if (await _context.Manufacturers.AnyAsync(m => m.Name.ToLower() == createManufacturerDto.Name.ToLower()))
        {
            return BadRequest("Já existe um fabricante com este nome");
        }

        var manufacturer = new Manufacturer
        {
            Name = createManufacturerDto.Name,
            ContactName = createManufacturerDto.ContactName,
            Email = createManufacturerDto.Email,
            Phone = createManufacturerDto.Phone,
            Address = createManufacturerDto.Address,
            Country = createManufacturerDto.Country,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Manufacturers.Add(manufacturer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetManufacturer), new { id = manufacturer.Id }, MapToManufacturerDto(manufacturer));
    }

    /// <summary>
    /// Atualiza um fabricante existente
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<IActionResult> UpdateManufacturer(int id, [FromBody] UpdateManufacturerDto updateManufacturerDto)
    {
        var manufacturer = await _context.Manufacturers.FindAsync(id);

        if (manufacturer == null)
        {
            return NotFound("Fabricante não encontrado");
        }

        // Verificar se outro fabricante já tem esse nome
        if (await _context.Manufacturers.AnyAsync(m => m.Name.ToLower() == updateManufacturerDto.Name.ToLower() && m.Id != id))
        {
            return BadRequest("Já existe outro fabricante com este nome");
        }

        // Atualizar propriedades
        manufacturer.Name = updateManufacturerDto.Name;
        manufacturer.ContactName = updateManufacturerDto.ContactName;
        manufacturer.Email = updateManufacturerDto.Email;
        manufacturer.Phone = updateManufacturerDto.Phone;
        manufacturer.Address = updateManufacturerDto.Address;
        manufacturer.Country = updateManufacturerDto.Country;
        manufacturer.IsActive = updateManufacturerDto.IsActive;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Remove um fabricante (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteManufacturer(int id)
    {
        var manufacturer = await _context.Manufacturers
            .Include(m => m.Vehicles)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (manufacturer == null)
        {
            return NotFound("Fabricante não encontrado");
        }

        // Verificar se o fabricante tem veículos associados
        if (manufacturer.Vehicles.Any())
        {
            return BadRequest("Não é possível excluir fabricante com veículos associados. Desative o fabricante em vez de excluí-lo.");
        }

        // Soft delete - marcar como inativo
        manufacturer.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Desativa um fabricante
    /// </summary>
    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<IActionResult> DeactivateManufacturer(int id)
    {
        var manufacturer = await _context.Manufacturers.FindAsync(id);

        if (manufacturer == null)
        {
            return NotFound("Fabricante não encontrado");
        }

        manufacturer.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Reativa um fabricante
    /// </summary>
    [HttpPatch("{id}/activate")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<IActionResult> ActivateManufacturer(int id)
    {
        var manufacturer = await _context.Manufacturers.FindAsync(id);

        if (manufacturer == null)
        {
            return NotFound("Fabricante não encontrado");
        }

        manufacturer.IsActive = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static ManufacturerDto MapToManufacturerDto(Manufacturer manufacturer)
    {
        return new ManufacturerDto
        {
            Id = manufacturer.Id,
            Name = manufacturer.Name,
            ContactName = manufacturer.ContactName,
            Email = manufacturer.Email,
            Phone = manufacturer.Phone,
            Address = manufacturer.Address,
            Country = manufacturer.Country,
            IsActive = manufacturer.IsActive,
            CreatedAt = manufacturer.CreatedAt
        };
    }
}