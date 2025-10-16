using DealershipInventorySystem.Domain.Entities;
using DealershipInventorySystem.Domain.Enums;

namespace DealershipInventorySystem.Tests.Domain;

public class VehicleTests
{
    [Fact]
    public void Vehicle_Creation_Should_Set_Default_Values()
    {
        // Arrange & Act
        var vehicle = new Vehicle();

        // Assert
        Assert.Equal(VehicleStatus.Available, vehicle.Status);
        Assert.True(vehicle.ArrivalDate <= DateTime.UtcNow);
        Assert.True(vehicle.ArrivalDate >= DateTime.UtcNow.AddMinutes(-1)); // Within last minute
        Assert.Empty(vehicle.Make);
        Assert.Empty(vehicle.Model);
        Assert.Empty(vehicle.Vin);
    }

    [Fact]
    public void Vehicle_With_Valid_Properties_Should_Be_Created_Successfully()
    {
        // Arrange
        const string expectedVin = "1HGBH41JXMN109186";
        const string expectedMake = "Ford";
        const string expectedModel = "Mustang";
        const int expectedYear = 2023;
        const string expectedColor = "Red";
        const decimal expectedCostPrice = 50000.00m;
        const decimal expectedSellingPrice = 65000.00m;

        // Act
        var vehicle = new Vehicle
        {
            Vin = expectedVin,
            Make = expectedMake,
            Model = expectedModel,
            Year = expectedYear,
            Color = expectedColor,
            Type = VehicleType.New,
            Status = VehicleStatus.Available,
            CostPrice = expectedCostPrice,
            SellingPrice = expectedSellingPrice,
            ManufacturerId = 1
        };

        // Assert
        Assert.Equal(expectedVin, vehicle.Vin);
        Assert.Equal(expectedMake, vehicle.Make);
        Assert.Equal(expectedModel, vehicle.Model);
        Assert.Equal(expectedYear, vehicle.Year);
        Assert.Equal(expectedColor, vehicle.Color);
        Assert.Equal(VehicleType.New, vehicle.Type);
        Assert.Equal(VehicleStatus.Available, vehicle.Status);
        Assert.Equal(expectedCostPrice, vehicle.CostPrice);
        Assert.Equal(expectedSellingPrice, vehicle.SellingPrice);
        Assert.Equal(1, vehicle.ManufacturerId);
    }

    [Fact]
    public void Vehicle_Status_Change_Should_Update_Correctly()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Status = VehicleStatus.Available
        };

        // Act
        vehicle.Status = VehicleStatus.Sold;

        // Assert
        Assert.Equal(VehicleStatus.Sold, vehicle.Status);
    }

    [Fact]
    public void Vehicle_Price_Should_Allow_Positive_Values()
    {
        // Arrange
        var vehicle = new Vehicle();

        // Act
        vehicle.CostPrice = 45000.50m;
        vehicle.SellingPrice = 55000.75m;

        // Assert
        Assert.Equal(45000.50m, vehicle.CostPrice);
        Assert.Equal(55000.75m, vehicle.SellingPrice);
    }

    [Theory]
    [InlineData(VehicleType.New)]
    [InlineData(VehicleType.SemiNew)]
    [InlineData(VehicleType.Used)]
    public void Vehicle_Type_Should_Accept_All_Valid_Types(VehicleType vehicleType)
    {
        // Arrange
        var vehicle = new Vehicle();

        // Act
        vehicle.Type = vehicleType;

        // Assert
        Assert.Equal(vehicleType, vehicle.Type);
    }

    [Theory]
    [InlineData(VehicleStatus.Available)]
    [InlineData(VehicleStatus.Reserved)]
    [InlineData(VehicleStatus.TestDrive)]
    [InlineData(VehicleStatus.Maintenance)]
    [InlineData(VehicleStatus.Sold)]
    [InlineData(VehicleStatus.Inactive)]
    public void Vehicle_Status_Should_Accept_All_Valid_Statuses(VehicleStatus status)
    {
        // Arrange
        var vehicle = new Vehicle();

        // Act
        vehicle.Status = status;

        // Assert
        Assert.Equal(status, vehicle.Status);
    }

    [Fact]
    public void Vehicle_Sold_Should_Set_SoldDate()
    {
        // Arrange
        var vehicle = new Vehicle();
        var soldDate = DateTime.UtcNow;

        // Act
        vehicle.Status = VehicleStatus.Sold;
        vehicle.SoldDate = soldDate;

        // Assert
        Assert.Equal(VehicleStatus.Sold, vehicle.Status);
        Assert.Equal(soldDate, vehicle.SoldDate);
    }

    [Fact]
    public void Vehicle_Movements_Collection_Should_Initialize_Empty()
    {
        // Arrange & Act
        var vehicle = new Vehicle();

        // Assert
        Assert.NotNull(vehicle.Movements);
        Assert.Empty(vehicle.Movements);
    }
}