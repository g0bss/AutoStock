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

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                context.HostingEnvironment.EnvironmentName = "Testing";
            });
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
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        await SeedTestUserAsync();

        var loginDto = new LoginDto
        {
            UserName = "testuser",
            Password = "Test123!"
        };

        var json = JsonSerializer.Serialize(loginDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginResponse);
        Assert.NotEmpty(loginResponse.Token);
        Assert.NotNull(loginResponse.User);
        Assert.Equal("testuser", loginResponse.User.UserName);
        Assert.True(loginResponse.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        await SeedTestUserAsync();

        var loginDto = new LoginDto
        {
            UserName = "testuser",
            Password = "WrongPassword"
        };

        var json = JsonSerializer.Serialize(loginDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            UserName = "nonexistent",
            Password = "Test123!"
        };

        var json = JsonSerializer.Serialize(loginDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            UserName = "newuser",
            Email = "newuser@dealership.com",
            FirstName = "New",
            LastName = "User",
            Password = "NewUser123!",
            Role = UserRole.Salesperson
        };

        var json = JsonSerializer.Serialize(createUserDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var userResponse = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(userResponse);
        Assert.Equal(createUserDto.UserName, userResponse.UserName);
        Assert.Equal(createUserDto.Email, userResponse.Email);
        Assert.Equal(createUserDto.FirstName, userResponse.FirstName);
        Assert.Equal(createUserDto.LastName, userResponse.LastName);
        Assert.Equal(createUserDto.Role, userResponse.Role);
    }

    [Fact]
    public async Task Register_WithDuplicateUserName_ShouldReturnBadRequest()
    {
        // Arrange
        await SeedTestUserAsync();

        var createUserDto = new CreateUserDto
        {
            UserName = "testuser", // Duplicate username
            Email = "another@dealership.com",
            FirstName = "Another",
            LastName = "User",
            Password = "AnotherUser123!",
            Role = UserRole.Salesperson
        };

        var json = JsonSerializer.Serialize(createUserDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        await SeedTestUserAsync();

        var createUserDto = new CreateUserDto
        {
            UserName = "anotheruser",
            Email = "test@dealership.com", // Duplicate email
            FirstName = "Another",
            LastName = "User",
            Password = "AnotherUser123!",
            Role = UserRole.Salesperson
        };

        var json = JsonSerializer.Serialize(createUserDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldUpdateLastLoginAt()
    {
        // Arrange
        await SeedTestUserAsync();

        var loginDto = new LoginDto
        {
            UserName = "testuser",
            Password = "Test123!"
        };

        var json = JsonSerializer.Serialize(loginDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify LastLoginAt was updated
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DealershipDbContext>();
        var user = await context.Users.FirstAsync(u => u.UserName == "testuser");

        Assert.NotNull(user.LastLoginAt);
        Assert.True(user.LastLoginAt >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task Login_WithInactiveUser_ShouldReturnUnauthorized()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DealershipDbContext>();

        var inactiveUser = new User
        {
            UserName = "inactiveuser",
            Email = "inactive@dealership.com",
            FirstName = "Inactive",
            LastName = "User",
            Role = UserRole.Salesperson,
            PasswordHash = HashPassword("Test123!"),
            IsActive = false, // Inactive user
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(inactiveUser);
        await context.SaveChangesAsync();

        var loginDto = new LoginDto
        {
            UserName = "inactiveuser",
            Password = "Test123!"
        };

        var json = JsonSerializer.Serialize(loginDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task SeedTestUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DealershipDbContext>();

        if (!await context.Users.AnyAsync(u => u.UserName == "testuser"))
        {
            var testUser = new User
            {
                UserName = "testuser",
                Email = "test@dealership.com",
                FirstName = "Test",
                LastName = "User",
                Role = UserRole.Salesperson,
                PasswordHash = HashPassword("Test123!"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(testUser);
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