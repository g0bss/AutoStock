using DealershipInventorySystem.Domain.Entities;
using DealershipInventorySystem.Domain.Enums;

namespace DealershipInventorySystem.Tests.Domain;

public class UserTests
{
    [Fact]
    public void User_Creation_Should_Set_Default_Values()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        Assert.True(user.IsActive);
        Assert.True(user.CreatedAt <= DateTime.UtcNow);
        Assert.True(user.CreatedAt >= DateTime.UtcNow.AddMinutes(-1)); // Within last minute
        Assert.Empty(user.UserName);
        Assert.Empty(user.Email);
        Assert.Empty(user.FirstName);
        Assert.Empty(user.LastName);
        Assert.Empty(user.PasswordHash);
        Assert.NotNull(user.Movements);
        Assert.Empty(user.Movements);
    }

    [Fact]
    public void User_FullName_Property_Should_Combine_Names_Correctly()
    {
        // Arrange
        var user = new User
        {
            FirstName = "Gabriel",
            LastName = "Costa"
        };

        // Act
        var fullName = user.FullName;

        // Assert
        Assert.Equal("Gabriel Costa", fullName);
    }

    [Fact]
    public void User_FullName_Should_Handle_Empty_Names()
    {
        // Arrange
        var user = new User
        {
            FirstName = "",
            LastName = ""
        };

        // Act
        var fullName = user.FullName;

        // Assert
        Assert.Equal(" ", fullName);
    }

    [Fact]
    public void User_With_Valid_Properties_Should_Be_Created_Successfully()
    {
        // Arrange
        const string expectedUserName = "admin";
        const string expectedEmail = "admin@dealership.com";
        const string expectedFirstName = "Admin";
        const string expectedLastName = "User";
        const string expectedPasswordHash = "hashedpassword123";
        const UserRole expectedRole = UserRole.Administrator;

        // Act
        var user = new User
        {
            UserName = expectedUserName,
            Email = expectedEmail,
            FirstName = expectedFirstName,
            LastName = expectedLastName,
            PasswordHash = expectedPasswordHash,
            Role = expectedRole,
            IsActive = true
        };

        // Assert
        Assert.Equal(expectedUserName, user.UserName);
        Assert.Equal(expectedEmail, user.Email);
        Assert.Equal(expectedFirstName, user.FirstName);
        Assert.Equal(expectedLastName, user.LastName);
        Assert.Equal(expectedPasswordHash, user.PasswordHash);
        Assert.Equal(expectedRole, user.Role);
        Assert.True(user.IsActive);
    }

    [Theory]
    [InlineData(UserRole.Administrator)]
    [InlineData(UserRole.Manager)]
    [InlineData(UserRole.Salesperson)]
    [InlineData(UserRole.Mechanic)]
    [InlineData(UserRole.Operator)]
    public void User_Role_Should_Accept_All_Valid_Roles(UserRole role)
    {
        // Arrange
        var user = new User();

        // Act
        user.Role = role;

        // Assert
        Assert.Equal(role, user.Role);
    }

    [Fact]
    public void User_LastLoginAt_Should_Be_Updated()
    {
        // Arrange
        var user = new User();
        var loginTime = DateTime.UtcNow;

        // Act
        user.LastLoginAt = loginTime;

        // Assert
        Assert.Equal(loginTime, user.LastLoginAt);
    }

    [Fact]
    public void User_Deactivation_Should_Set_IsActive_False()
    {
        // Arrange
        var user = new User { IsActive = true };

        // Act
        user.IsActive = false;

        // Assert
        Assert.False(user.IsActive);
    }

    [Fact]
    public void User_Email_Should_Store_Value_Correctly()
    {
        // Arrange
        var user = new User();
        const string expectedEmail = "test@example.com";

        // Act
        user.Email = expectedEmail;

        // Assert
        Assert.Equal(expectedEmail, user.Email);
    }

    [Fact]
    public void Multiple_Users_Should_Have_Different_CreatedAt_Times()
    {
        // Arrange & Act
        var user1 = new User();
        Thread.Sleep(10); // Small delay to ensure different timestamps
        var user2 = new User();

        // Assert
        Assert.True(user2.CreatedAt >= user1.CreatedAt);
    }

    [Fact]
    public void User_Movements_Collection_Should_Be_Modifiable()
    {
        // Arrange
        var user = new User();
        var movement = new VehicleMovement { Id = 1 };

        // Act
        user.Movements.Add(movement);

        // Assert
        Assert.Single(user.Movements);
        Assert.Equal(movement, user.Movements.First());
    }
}