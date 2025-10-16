using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DealershipInventorySystem.Application.DTOs;
using DealershipInventorySystem.Domain.Entities;
using DealershipInventorySystem.Domain.Enums;
using DealershipInventorySystem.Infrastructure.Data;
using DealershipInventorySystem.WebAPI;

namespace DealershipInventorySystem.Tests.Controllers;

public class VehiclesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public VehiclesControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app's DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<DealershipDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<DealershipDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetVehicles_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/vehicles");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetVehicles_WithAuthentication_ShouldReturnOk()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/api/vehicles");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var vehicles = await response.Content.ReadFromJsonAsync<List<VehicleDto>>();
        Assert.NotNull(vehicles);
        Assert.NotEmpty(vehicles);
    }

    [Fact]
    public async Task GetVehicle_WithValidId_ShouldReturnVehicle()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var vehicleId = await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync($"/api/vehicles/{vehicleId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var vehicle = await response.Content.ReadFromJsonAsync<VehicleDto>();
        Assert.NotNull(vehicle);
        Assert.Equal(vehicleId, vehicle.Id);
    }

    [Fact]
    public async Task GetVehicle_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/vehicles/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateVehicle_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        await EnsureManufacturerExistsAsync();

        var createVehicleDto = new CreateVehicleDto
        {
            Vin = "1HGBH41JXMN109187",
            Make = "Ford",
            Model = "Focus",
            Year = 2023,
            Color = "Blue",
            Type = VehicleType.New,
            FuelType = FuelType.Gasoline,
            TransmissionType = TransmissionType.Automatic,
            Mileage = 0,
            CostPrice = 45000.00m,
            SellingPrice = 52000.00m,
            Notes = "New arrival",
            ManufacturerId = 1
        };

        var json = JsonSerializer.Serialize(createVehicleDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/vehicles", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdVehicle = await response.Content.ReadFromJsonAsync<VehicleDto>();
        Assert.NotNull(createdVehicle);
        Assert.Equal(createVehicleDto.Vin, createdVehicle.Vin);
        Assert.Equal(createVehicleDto.Make, createdVehicle.Make);
        Assert.Equal(createVehicleDto.Model, createdVehicle.Model);
    }

    [Fact]
    public async Task CreateVehicle_WithDuplicateVin_ShouldReturnBadRequest()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        await SeedTestDataAsync(); // This creates a vehicle with VIN "1HGBH41JXMN109186"

        var createVehicleDto = new CreateVehicleDto
        {
            Vin = "1HGBH41JXMN109186", // Duplicate VIN
            Make = "Ford",
            Model = "Focus",
            Year = 2023,
            Color = "Blue",
            Type = VehicleType.New,
            FuelType = FuelType.Gasoline,
            TransmissionType = TransmissionType.Automatic,
            ManufacturerId = 1
        };

        var json = JsonSerializer.Serialize(createVehicleDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/vehicles", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<string> GetAuthTokenAsync()
    {
        // First, ensure admin user exists
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DealershipDbContext>();

        if (!await context.Users.AnyAsync())
        {
            var adminUser = new User
            {
                UserName = "admin",
                Email = "admin@dealership.com",
                FirstName = "Admin",
                LastName = "User",
                Role = UserRole.Administrator,
                PasswordHash = HashPassword("Admin123!"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }

        // Login to get token
        var loginDto = new LoginDto
        {
            UserName = "admin",
            Password = "Admin123!"
        };

        var json = JsonSerializer.Serialize(loginDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/login", content);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

        return loginResponse!.Token;
    }

    private async Task<int> SeedTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DealershipDbContext>();

        // Ensure manufacturer exists
        if (!await context.Manufacturers.AnyAsync())
        {
            context.Manufacturers.Add(new Manufacturer
            {
                Id = 1,
                Name = "Ford",
                ContactName = "Ford Brasil",
                Email = "contato@ford.com.br",
                Country = "Brasil",
                IsActive = true
            });
            await context.SaveChangesAsync();
        }

        // Add test vehicle
        if (!await context.Vehicles.AnyAsync())
        {
            var vehicle = new Vehicle
            {
                Vin = "1HGBH41JXMN109186",
                Make = "Ford",
                Model = "Mustang",
                Year = 2023,
                Color = "Red",
                Type = VehicleType.New,
                Status = VehicleStatus.Available,
                FuelType = FuelType.Gasoline,
                TransmissionType = TransmissionType.Automatic,
                Mileage = 0,
                CostPrice = 50000.00m,
                SellingPrice = 65000.00m,
                ManufacturerId = 1,
                ArrivalDate = DateTime.UtcNow
            };

            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync();
            return vehicle.Id;
        }

        var existingVehicle = await context.Vehicles.FirstAsync();
        return existingVehicle.Id;
    }

    private async Task EnsureManufacturerExistsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DealershipDbContext>();

        if (!await context.Manufacturers.AnyAsync(m => m.Id == 1))
        {
            context.Manufacturers.Add(new Manufacturer
            {
                Id = 1,
                Name = "Ford",
                ContactName = "Ford Brasil",
                Email = "contato@ford.com.br",
                Country = "Brasil",
                IsActive = true
            });
            await context.SaveChangesAsync();
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}