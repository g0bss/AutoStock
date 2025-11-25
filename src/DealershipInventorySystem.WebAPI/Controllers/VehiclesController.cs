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
public class VehiclesController : ControllerBase
{
    private readonly DealershipDbContext _context;

    public VehiclesController(DealershipDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém todos os veículos
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetVehicles()
    {
        var vehicles = await _context.Vehicles
            .Include(v => v.Manufacturer)
            .Include(v => v.Customer)
            .Select(v => MapToVehicleDto(v))
            .ToListAsync();

        return Ok(vehicles);
    }

    /// <summary>
    /// Obtém um veículo específico por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<VehicleDto>> GetVehicle(int id)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.Manufacturer)
            .Include(v => v.Customer)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle == null)
        {
            return NotFound();
        }

        return Ok(MapToVehicleDto(vehicle));
    }

    /// <summary>
    /// Obtém veículo por VIN/Chassi
    /// </summary>
    [HttpGet("vin/{vin}")]
    public async Task<ActionResult<VehicleDto>> GetVehicleByVin(string vin)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.Manufacturer)
            .Include(v => v.Customer)
            .FirstOrDefaultAsync(v => v.Vin == vin);

        if (vehicle == null)
        {
            return NotFound();
        }

        return Ok(MapToVehicleDto(vehicle));
    }

    /// <summary>
    /// Cria um novo veículo
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<ActionResult<VehicleDto>> CreateVehicle([FromBody] CreateVehicleDto createVehicleDto)
    {
        // Verificar se o VIN já existe
        if (await _context.Vehicles.AnyAsync(v => v.Vin == createVehicleDto.Vin))
        {
            return BadRequest("Veículo com este VIN/Chassi já existe");
        }

        // Verificar se o fabricante existe
        if (!await _context.Manufacturers.AnyAsync(m => m.Id == createVehicleDto.ManufacturerId))
        {
            return BadRequest("Fabricante não encontrado");
        }

        var vehicle = new Vehicle
        {
            Vin = createVehicleDto.Vin,
            Make = createVehicleDto.Make,
            Model = createVehicleDto.Model,
            Year = createVehicleDto.Year,
            Color = createVehicleDto.Color,
            Type = createVehicleDto.Type,
            Status = createVehicleDto.Status,
            FuelType = createVehicleDto.FuelType,
            TransmissionType = createVehicleDto.TransmissionType,
            Mileage = createVehicleDto.Mileage,
            CostPrice = createVehicleDto.CostPrice,
            SellingPrice = createVehicleDto.SellingPrice,
            Notes = createVehicleDto.Notes,
            ManufacturerId = createVehicleDto.ManufacturerId,
            ArrivalDate = DateTime.UtcNow
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var createdVehicle = await _context.Vehicles
            .Include(v => v.Manufacturer)
            .FirstOrDefaultAsync(v => v.Id == vehicle.Id);

        return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, MapToVehicleDto(createdVehicle!));
    }

    /// <summary>
    /// Atualiza um veículo existente
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<IActionResult> UpdateVehicle(int id, [FromBody] UpdateVehicleDto updateVehicleDto)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);

        if (vehicle == null)
        {
            return NotFound();
        }

        // Verificar se o fabricante existe
        if (!await _context.Manufacturers.AnyAsync(m => m.Id == updateVehicleDto.ManufacturerId))
        {
            return BadRequest("Fabricante não encontrado");
        }

        // Atualizar propriedades
        vehicle.Make = updateVehicleDto.Make;
        vehicle.Model = updateVehicleDto.Model;
        vehicle.Year = updateVehicleDto.Year;
        vehicle.Color = updateVehicleDto.Color;
        vehicle.Type = updateVehicleDto.Type;
        vehicle.Status = updateVehicleDto.Status;
        vehicle.FuelType = updateVehicleDto.FuelType;
        vehicle.TransmissionType = updateVehicleDto.TransmissionType;
        vehicle.Mileage = updateVehicleDto.Mileage;
        vehicle.CostPrice = updateVehicleDto.CostPrice;
        vehicle.SellingPrice = updateVehicleDto.SellingPrice;
        vehicle.Notes = updateVehicleDto.Notes;
        vehicle.ManufacturerId = updateVehicleDto.ManufacturerId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Exclui um veículo
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteVehicle(int id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);

        if (vehicle == null)
        {
            return NotFound();
        }

        // Verificar se o veículo tem movimentações
        if (await _context.VehicleMovements.AnyAsync(vm => vm.VehicleId == id))
        {
            return BadRequest("Não é possível excluir veículo com histórico de movimentações");
        }

        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static VehicleDto MapToVehicleDto(Vehicle vehicle)
    {
        return new VehicleDto
        {
            Id = vehicle.Id,
            Vin = vehicle.Vin,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Color = vehicle.Color,
            Type = vehicle.Type,
            Status = vehicle.Status,
            FuelType = vehicle.FuelType,
            TransmissionType = vehicle.TransmissionType,
            Mileage = vehicle.Mileage,
            CostPrice = vehicle.CostPrice,
            SellingPrice = vehicle.SellingPrice,
            ArrivalDate = vehicle.ArrivalDate,
            SoldDate = vehicle.SoldDate,
            Notes = vehicle.Notes,
            ManufacturerId = vehicle.ManufacturerId,
            ManufacturerName = vehicle.Manufacturer?.Name ?? "",
            CustomerId = vehicle.CustomerId,
            CustomerName = vehicle.Customer?.Name
        };
    }
}