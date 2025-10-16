using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DealershipInventorySystem.Application.DTOs;
using DealershipInventorySystem.Domain.Entities;
using DealershipInventorySystem.Domain.Enums;
using DealershipInventorySystem.Infrastructure.Data;

namespace DealershipInventorySystem.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly DealershipDbContext _context;

    public UsersController(DealershipDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém todos os usuários (apenas Administradores)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _context.Users
            .Where(u => u.IsActive)
            .Select(u => MapToUserDto(u))
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Obtém um usuário específico por ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound("Usuário não encontrado");
        }

        // Usuários comuns só podem ver seu próprio perfil
        var currentUserId = GetCurrentUserId();
        var currentUserRole = GetCurrentUserRole();

        if (currentUserRole != "Administrator" && currentUserRole != "Manager" && currentUserId != id)
        {
            return Forbid("Você não tem permissão para ver este usuário");
        }

        return Ok(MapToUserDto(user));
    }

    /// <summary>
    /// Obtém o perfil do usuário atual
    /// </summary>
    [HttpGet("profile")]
    public async Task<ActionResult<UserDto>> GetCurrentUserProfile()
    {
        var currentUserId = GetCurrentUserId();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);

        if (user == null)
        {
            return NotFound("Usuário não encontrado");
        }

        return Ok(MapToUserDto(user));
    }

    /// <summary>
    /// Busca usuários por nome ou username
    /// </summary>
    [HttpGet("search")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<ActionResult<IEnumerable<UserDto>>> SearchUsers([FromQuery] string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return BadRequest("Termo de busca é obrigatório");
        }

        var users = await _context.Users
            .Where(u => u.IsActive &&
                       (u.UserName.ToLower().Contains(term.ToLower()) ||
                        u.FirstName.ToLower().Contains(term.ToLower()) ||
                        u.LastName.ToLower().Contains(term.ToLower()) ||
                        u.Email.ToLower().Contains(term.ToLower())))
            .Select(u => MapToUserDto(u))
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Obtém usuários por papel/função
    /// </summary>
    [HttpGet("by-role/{role}")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByRole(UserRole role)
    {
        var users = await _context.Users
            .Where(u => u.IsActive && u.Role == role)
            .Select(u => MapToUserDto(u))
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Cria um novo usuário (apenas Administradores)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        // Verificar se usuário já existe
        if (await _context.Users.AnyAsync(u => u.UserName == createUserDto.UserName))
        {
            return BadRequest("Nome de usuário já existe");
        }

        if (await _context.Users.AnyAsync(u => u.Email == createUserDto.Email))
        {
            return BadRequest("Email já está em uso");
        }

        var user = new User
        {
            UserName = createUserDto.UserName,
            Email = createUserDto.Email,
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            Role = createUserDto.Role,
            PasswordHash = HashPassword(createUserDto.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, MapToUserDto(user));
    }

    /// <summary>
    /// Atualiza um usuário existente
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound("Usuário não encontrado");
        }

        var currentUserId = GetCurrentUserId();
        var currentUserRole = GetCurrentUserRole();

        // Verificar permissões
        if (currentUserRole == "Manager" && user.Role == UserRole.Administrator)
        {
            return Forbid("Gerentes não podem modificar Administradores");
        }

        // Usuários comuns só podem atualizar seu próprio perfil e não podem mudar role
        if (currentUserRole != "Administrator" && currentUserRole != "Manager")
        {
            if (currentUserId != id)
            {
                return Forbid("Você só pode atualizar seu próprio perfil");
            }
            // Para usuários comuns, manter o role atual
            updateUserDto = updateUserDto with { Role = user.Role };
        }

        // Verificar se outro usuário já tem esse email
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == updateUserDto.Email.ToLower() && u.Id != id))
        {
            return BadRequest("Outro usuário já utiliza este email");
        }

        // Atualizar propriedades
        user.Email = updateUserDto.Email;
        user.FirstName = updateUserDto.FirstName;
        user.LastName = updateUserDto.LastName;
        user.Role = updateUserDto.Role;
        user.IsActive = updateUserDto.IsActive;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Desativa um usuário
    /// </summary>
    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound("Usuário não encontrado");
        }

        var currentUserId = GetCurrentUserId();

        // Impedir auto-desativação
        if (currentUserId == id)
        {
            return BadRequest("Você não pode desativar sua própria conta");
        }

        user.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Reativa um usuário
    /// </summary>
    [HttpPatch("{id}/activate")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> ActivateUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound("Usuário não encontrado");
        }

        user.IsActive = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Altera a senha do usuário
    /// </summary>
    [HttpPatch("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto changePasswordDto)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound("Usuário não encontrado");
        }

        var currentUserId = GetCurrentUserId();
        var currentUserRole = GetCurrentUserRole();

        // Verificar permissões - usuários só podem mudar sua própria senha ou administradores podem mudar qualquer senha
        if (currentUserRole != "Administrator" && currentUserId != id)
        {
            return Forbid("Você só pode alterar sua própria senha");
        }

        // Se não for administrador, verificar senha atual
        if (currentUserRole != "Administrator")
        {
            if (!VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                return BadRequest("Senha atual incorreta");
            }
        }

        // Atualizar senha
        user.PasswordHash = HashPassword(changePasswordDto.NewPassword);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Obtém estatísticas de usuários
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<ActionResult<object>> GetUserStatistics()
    {
        var statistics = new
        {
            TotalUsers = await _context.Users.CountAsync(u => u.IsActive),
            InactiveUsers = await _context.Users.CountAsync(u => !u.IsActive),
            UsersByRole = await _context.Users
                .Where(u => u.IsActive)
                .GroupBy(u => u.Role)
                .Select(g => new { Role = g.Key.ToString(), Count = g.Count() })
                .ToListAsync(),
            RecentlyCreated = await _context.Users
                .Where(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                .CountAsync(),
            RecentlyActive = await _context.Users
                .Where(u => u.LastLoginAt >= DateTime.UtcNow.AddDays(-7))
                .CountAsync()
        };

        return Ok(statistics);
    }

    /// <summary>
    /// Obtém histórico de atividades do usuário
    /// </summary>
    [HttpGet("{id}/activity")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<ActionResult<object>> GetUserActivity(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound("Usuário não encontrado");
        }

        // Obter movimentações criadas pelo usuário
        var movements = await _context.VehicleMovements
            .Where(vm => vm.UserId == id)
            .Include(vm => vm.Vehicle)
            .OrderByDescending(vm => vm.MovementDate)
            .Take(50)
            .Select(vm => new
            {
                vm.Id,
                vm.Type,
                vm.MovementDate,
                vm.Description,
                VehicleVin = vm.Vehicle.Vin,
                VehicleMakeModel = vm.Vehicle.Make + " " + vm.Vehicle.Model
            })
            .ToListAsync();

        var activity = new
        {
            UserId = user.Id,
            UserName = user.UserName,
            TotalMovements = await _context.VehicleMovements.CountAsync(vm => vm.UserId == id),
            RecentMovements = movements,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };

        return Ok(activity);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim?.Value ?? "0");
    }

    private string GetCurrentUserRole()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role);
        return roleClaim?.Value ?? "";
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        var hashedInput = HashPassword(password);
        return hashedInput == hash;
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}

public record ChangePasswordDto
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}