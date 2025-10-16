using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DealershipInventorySystem.Application.DTOs;
using DealershipInventorySystem.Domain.Entities;
using DealershipInventorySystem.Domain.Enums;
using DealershipInventorySystem.Infrastructure.Data;

namespace DealershipInventorySystem.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VehicleMovementsController : ControllerBase
{
    private readonly DealershipDbContext _context;

    public VehicleMovementsController(DealershipDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém todas as movimentações de veículos
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehicleMovementDto>>> GetMovements(
        [FromQuery] int? vehicleId = null,
        [FromQuery] int? customerId = null,
        [FromQuery] MovementType? type = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _context.VehicleMovements
            .Include(vm => vm.Vehicle)
                .ThenInclude(v => v.Manufacturer)
            .Include(vm => vm.Customer)
            .Include(vm => vm.User)
            .AsQueryable();

        // Aplicar filtros
        if (vehicleId.HasValue)
            query = query.Where(vm => vm.VehicleId == vehicleId);

        if (customerId.HasValue)
            query = query.Where(vm => vm.CustomerId == customerId);

        if (type.HasValue)
            query = query.Where(vm => vm.Type == type);

        if (startDate.HasValue)
            query = query.Where(vm => vm.MovementDate >= startDate);

        if (endDate.HasValue)
            query = query.Where(vm => vm.MovementDate <= endDate);

        // Ordenar por data mais recente
        query = query.OrderByDescending(vm => vm.MovementDate);

        // Paginação
        var totalCount = await query.CountAsync();
        var movements = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(vm => MapToVehicleMovementDto(vm))
            .ToListAsync();

        Response.Headers["X-Total-Count"] = totalCount.ToString();
        Response.Headers["X-Page"] = page.ToString();
        Response.Headers["X-Page-Size"] = pageSize.ToString();

        return Ok(movements);
    }

    /// <summary>
    /// Obtém uma movimentação específica por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<VehicleMovementDto>> GetMovement(int id)
    {
        var movement = await _context.VehicleMovements
            .Include(vm => vm.Vehicle)
                .ThenInclude(v => v.Manufacturer)
            .Include(vm => vm.Customer)
            .Include(vm => vm.User)
            .FirstOrDefaultAsync(vm => vm.Id == id);

        if (movement == null)
        {
            return NotFound("Movimentação não encontrada");
        }

        return Ok(MapToVehicleMovementDto(movement));
    }

    /// <summary>
    /// Obtém o histórico completo de um veículo
    /// </summary>
    [HttpGet("vehicle/{vehicleId}/history")]
    public async Task<ActionResult<IEnumerable<VehicleMovementDto>>> GetVehicleHistory(int vehicleId)
    {
        var vehicle = await _context.Vehicles.FindAsync(vehicleId);
        if (vehicle == null)
        {
            return NotFound("Veículo não encontrado");
        }

        var movements = await _context.VehicleMovements
            .Where(vm => vm.VehicleId == vehicleId)
            .Include(vm => vm.Vehicle)
                .ThenInclude(v => v.Manufacturer)
            .Include(vm => vm.Customer)
            .Include(vm => vm.User)
            .OrderByDescending(vm => vm.MovementDate)
            .Select(vm => MapToVehicleMovementDto(vm))
            .ToListAsync();

        return Ok(movements);
    }

    /// <summary>
    /// Obtém movimentações por cliente
    /// </summary>
    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<VehicleMovementDto>>> GetCustomerMovements(int customerId)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null)
        {
            return NotFound("Cliente não encontrado");
        }

        var movements = await _context.VehicleMovements
            .Where(vm => vm.CustomerId == customerId)
            .Include(vm => vm.Vehicle)
                .ThenInclude(v => v.Manufacturer)
            .Include(vm => vm.Customer)
            .Include(vm => vm.User)
            .OrderByDescending(vm => vm.MovementDate)
            .Select(vm => MapToVehicleMovementDto(vm))
            .ToListAsync();

        return Ok(movements);
    }

    /// <summary>
    /// Cria uma nova movimentação
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<VehicleMovementDto>> CreateMovement([FromBody] CreateVehicleMovementDto createMovementDto)
    {
        // Verificar se o veículo existe
        var vehicle = await _context.Vehicles.FindAsync(createMovementDto.VehicleId);
        if (vehicle == null)
        {
            return BadRequest("Veículo não encontrado");
        }

        // Verificar se o cliente existe (quando informado)
        if (createMovementDto.CustomerId.HasValue)
        {
            var customer = await _context.Customers.FindAsync(createMovementDto.CustomerId);
            if (customer == null)
            {
                return BadRequest("Cliente não encontrado");
            }
        }

        // Obter usuário atual
        var userId = GetCurrentUserId();

        var movement = new VehicleMovement
        {
            Type = createMovementDto.Type,
            Description = createMovementDto.Description,
            Value = createMovementDto.Value,
            Notes = createMovementDto.Notes,
            VehicleId = createMovementDto.VehicleId,
            CustomerId = createMovementDto.CustomerId,
            UserId = userId,
            MovementDate = DateTime.UtcNow
        };

        // Atualizar status do veículo baseado no tipo de movimentação
        await UpdateVehicleStatusBasedOnMovement(vehicle, createMovementDto.Type, createMovementDto.CustomerId);

        _context.VehicleMovements.Add(movement);
        await _context.SaveChangesAsync();

        var createdMovement = await _context.VehicleMovements
            .Include(vm => vm.Vehicle)
                .ThenInclude(v => v.Manufacturer)
            .Include(vm => vm.Customer)
            .Include(vm => vm.User)
            .FirstOrDefaultAsync(vm => vm.Id == movement.Id);

        return CreatedAtAction(nameof(GetMovement), new { id = movement.Id }, MapToVehicleMovementDto(createdMovement!));
    }

    /// <summary>
    /// Registra entrada de veículo no estoque
    /// </summary>
    [HttpPost("entry")]
    [Authorize(Roles = "Administrator,Manager,Operator")]
    public async Task<ActionResult<VehicleMovementDto>> RegisterVehicleEntry([FromBody] CreateVehicleMovementDto createMovementDto)
    {
        createMovementDto = createMovementDto with { Type = MovementType.Entry };
        return await CreateMovement(createMovementDto);
    }

    /// <summary>
    /// Registra test drive
    /// </summary>
    [HttpPost("test-drive")]
    [Authorize(Roles = "Administrator,Manager,Salesperson")]
    public async Task<ActionResult<VehicleMovementDto>> RegisterTestDrive([FromBody] CreateVehicleMovementDto createMovementDto)
    {
        if (!createMovementDto.CustomerId.HasValue)
        {
            return BadRequest("Cliente é obrigatório para test drive");
        }

        createMovementDto = createMovementDto with { Type = MovementType.TestDrive };
        return await CreateMovement(createMovementDto);
    }

    /// <summary>
    /// Registra reserva de veículo
    /// </summary>
    [HttpPost("reservation")]
    [Authorize(Roles = "Administrator,Manager,Salesperson")]
    public async Task<ActionResult<VehicleMovementDto>> RegisterReservation([FromBody] CreateVehicleMovementDto createMovementDto)
    {
        if (!createMovementDto.CustomerId.HasValue)
        {
            return BadRequest("Cliente é obrigatório para reserva");
        }

        createMovementDto = createMovementDto with { Type = MovementType.Reservation };
        return await CreateMovement(createMovementDto);
    }

    /// <summary>
    /// Registra venda de veículo
    /// </summary>
    [HttpPost("sale")]
    [Authorize(Roles = "Administrator,Manager,Salesperson")]
    public async Task<ActionResult<VehicleMovementDto>> RegisterSale([FromBody] CreateVehicleMovementDto createMovementDto)
    {
        if (!createMovementDto.CustomerId.HasValue)
        {
            return BadRequest("Cliente é obrigatório para venda");
        }

        if (!createMovementDto.Value.HasValue || createMovementDto.Value <= 0)
        {
            return BadRequest("Valor da venda é obrigatório e deve ser maior que zero");
        }

        createMovementDto = createMovementDto with { Type = MovementType.Sale };
        return await CreateMovement(createMovementDto);
    }

    /// <summary>
    /// Registra manutenção
    /// </summary>
    [HttpPost("maintenance")]
    [Authorize(Roles = "Administrator,Manager,Mechanic")]
    public async Task<ActionResult<VehicleMovementDto>> RegisterMaintenance([FromBody] CreateVehicleMovementDto createMovementDto)
    {
        createMovementDto = createMovementDto with { Type = MovementType.Maintenance };
        return await CreateMovement(createMovementDto);
    }

    /// <summary>
    /// Obtém relatório de movimentações por período
    /// </summary>
    [HttpGet("report")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<ActionResult<object>> GetMovementReport(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        var movements = await _context.VehicleMovements
            .Where(vm => vm.MovementDate >= start && vm.MovementDate <= end)
            .Include(vm => vm.Vehicle)
            .ToListAsync();

        var report = new
        {
            Period = new { Start = start, End = end },
            TotalMovements = movements.Count,
            MovementsByType = movements
                .GroupBy(vm => vm.Type)
                .Select(g => new { Type = g.Key.ToString(), Count = g.Count(), TotalValue = g.Sum(x => x.Value ?? 0) })
                .OrderByDescending(x => x.Count),
            TotalSalesValue = movements.Where(vm => vm.Type == MovementType.Sale).Sum(vm => vm.Value ?? 0),
            TotalMaintenanceCost = movements.Where(vm => vm.Type == MovementType.Maintenance).Sum(vm => vm.Value ?? 0),
            MovementsByDay = movements
                .GroupBy(vm => vm.MovementDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
        };

        return Ok(report);
    }

    /// <summary>
    /// Exclui uma movimentação (apenas Administradores)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteMovement(int id)
    {
        var movement = await _context.VehicleMovements.FindAsync(id);

        if (movement == null)
        {
            return NotFound("Movimentação não encontrada");
        }

        // Verificar se é uma venda - reverter status do veículo
        if (movement.Type == MovementType.Sale)
        {
            var vehicle = await _context.Vehicles.FindAsync(movement.VehicleId);
            if (vehicle != null)
            {
                vehicle.Status = VehicleStatus.Available;
                vehicle.CustomerId = null;
                vehicle.SoldDate = null;
            }
        }

        _context.VehicleMovements.Remove(movement);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private Task UpdateVehicleStatusBasedOnMovement(Vehicle vehicle, MovementType movementType, int? customerId)
    {
        switch (movementType)
        {
            case MovementType.Entry:
                vehicle.Status = VehicleStatus.Available;
                break;

            case MovementType.TestDrive:
                vehicle.Status = VehicleStatus.TestDrive;
                break;

            case MovementType.Reservation:
                vehicle.Status = VehicleStatus.Reserved;
                break;

            case MovementType.Sale:
                vehicle.Status = VehicleStatus.Sold;
                vehicle.CustomerId = customerId;
                vehicle.SoldDate = DateTime.UtcNow;
                break;

            case MovementType.Maintenance:
                vehicle.Status = VehicleStatus.Maintenance;
                break;

            case MovementType.Transfer:
                vehicle.Status = VehicleStatus.Available;
                break;
        }

        return Task.CompletedTask;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim?.Value ?? "0");
    }

    private static VehicleMovementDto MapToVehicleMovementDto(VehicleMovement movement)
    {
        return new VehicleMovementDto
        {
            Id = movement.Id,
            Type = movement.Type,
            MovementDate = movement.MovementDate,
            Description = movement.Description,
            Value = movement.Value,
            Notes = movement.Notes,
            VehicleId = movement.VehicleId,
            VehicleVin = movement.Vehicle.Vin,
            VehicleMakeModel = $"{movement.Vehicle.Make} {movement.Vehicle.Model}",
            CustomerId = movement.CustomerId,
            CustomerName = movement.Customer?.Name,
            UserId = movement.UserId,
            UserName = movement.User.UserName
        };
    }
}