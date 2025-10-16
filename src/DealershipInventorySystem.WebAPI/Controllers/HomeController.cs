using Microsoft.AspNetCore.Mvc;
using DealershipInventorySystem.Infrastructure.Data;

namespace DealershipInventorySystem.WebAPI.Controllers;

[ApiController]
[Route("api/info")]
public class HomeController : ControllerBase
{
    private readonly DealershipDbContext _context;

    public HomeController(DealershipDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Página inicial da API - Informações gerais
    /// </summary>
    [HttpGet("home")]
    public ActionResult GetHomePage()
    {
        var apiInfo = new
        {
            Title = "Sistema de Gerenciamento de Estoque - Concessionária de Automóveis",
            Version = "1.0.0",
            Description = "API completa para gerenciamento de inventário de concessionária automotiva",
            Developers = new[] { "Gabriel Ferreira Costa", "Eduardo Cabral Nunes" },
            Institution = "CEUB - Centro Universitário de Brasília",
            Course = "Ciência da Computação - Desenvolvimento de Sistemas",
            Status = "🟢 Online",
            Documentation = new
            {
                SwaggerUI = "/swagger",
                Description = "Documentação completa da API com interface interativa"
            },
            Authentication = new
            {
                Type = "JWT Bearer Token",
                LoginEndpoint = "/api/auth/login",
                DefaultCredentials = new
                {
                    Username = "admin",
                    Password = "Admin123!",
                    Note = "Use estas credenciais para testar a API"
                }
            },
            MainEndpoints = new
            {
                Auth = "/api/auth",
                Vehicles = "/api/vehicles",
                Manufacturers = "/api/manufacturers",
                Customers = "/api/customers",
                VehicleMovements = "/api/vehiclemovements",
                Users = "/api/users"
            },
            TestEndpoints = new
            {
                Health = "/api/health",
                Info = "/api/info",
                Status = "/api/status"
            },
            Technologies = new[]
            {
                ".NET 9.0",
                "ASP.NET Core Web API",
                "Entity Framework Core",
                "PostgreSQL / In-Memory Database",
                "JWT Authentication",
                "Swagger/OpenAPI",
                "Docker",
                "xUnit Testing"
            }
        };

        return Ok(apiInfo);
    }

    /// <summary>
    /// Status de saúde da API
    /// </summary>
    [HttpGet("health")]
    public ActionResult GetHealth()
    {
        try
        {
            // Test database connection
            var manufacturersCount = _context.Manufacturers.Count();
            var usersCount = _context.Users.Count();
            var vehiclesCount = _context.Vehicles.Count();

            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Uptime = "Running",
                Database = new
                {
                    Status = "Connected",
                    Type = "In-Memory",
                    Manufacturers = manufacturersCount,
                    Users = usersCount,
                    Vehicles = vehiclesCount
                },
                Environment = "Development",
                Version = "1.0.0"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Status = "Unhealthy",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Informações detalhadas da API
    /// </summary>
    [HttpGet("details")]
    public ActionResult GetInfo()
    {
        return Ok(new
        {
            ApiName = "Dealership Inventory System API",
            Version = "1.0.0",
            BuildDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            Environment = "Development",
            Framework = ".NET 9.0",
            Database = "In-Memory (Development)",
            Authentication = "JWT Bearer",
            Features = new[]
            {
                "Complete CRUD operations for all entities",
                "Role-based authorization",
                "JWT authentication with refresh",
                "Comprehensive error handling",
                "Request logging and correlation IDs",
                "Swagger documentation",
                "Automated testing suite (51 tests)",
                "Clean Architecture implementation"
            },
            Endpoints = new
            {
                TotalControllers = 6,
                AuthEndpoints = 3,
                VehicleEndpoints = 5,
                ManufacturerEndpoints = 7,
                CustomerEndpoints = 9,
                MovementEndpoints = 10,
                UserEndpoints = 8
            },
            TestCredentials = new
            {
                Admin = new { Username = "admin", Password = "Admin123!" },
                Note = "Use admin credentials to access protected endpoints"
            }
        });
    }

    /// <summary>
    /// Status detalhado da aplicação
    /// </summary>
    [HttpGet("status")]
    public ActionResult GetStatus()
    {
        return Ok(new
        {
            Application = new
            {
                Name = "DealershipInventorySystem.WebAPI",
                Status = "Running",
                Port = 5195,
                StartTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                Environment = "Development"
            },
            Database = new
            {
                Provider = "InMemory",
                Status = "Connected",
                SeedDataLoaded = true
            },
            Authentication = new
            {
                Type = "JWT",
                Status = "Enabled",
                TokenExpiration = "24 hours"
            },
            Features = new
            {
                Swagger = "Enabled - /swagger",
                CORS = "Enabled - AllowAll",
                RequestLogging = "Enabled",
                ErrorHandling = "Enabled",
                Authorization = "Enabled"
            },
            QuickStart = new
            {
                Step1 = "Go to /swagger for API documentation",
                Step2 = "Login with admin/Admin123! at /api/auth/login",
                Step3 = "Copy the returned JWT token",
                Step4 = "Use 'Bearer {token}' in Authorization header",
                Step5 = "Access protected endpoints like /api/vehicles"
            }
        });
    }
}