using Microsoft.EntityFrameworkCore;
using DealershipInventorySystem.Domain.Entities;

namespace DealershipInventorySystem.Infrastructure.Data;

public class DealershipDbContext : DbContext
{
    public DealershipDbContext(DbContextOptions<DealershipDbContext> options) : base(options)
    {
    }

    // DbSets para as entidades principais
    public DbSet<Vehicle> Vehicles { get; set; } = null!;
    public DbSet<Manufacturer> Manufacturers { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<VehicleMovement> VehicleMovements { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração das entidades
        ConfigureVehicle(modelBuilder);
        ConfigureManufacturer(modelBuilder);
        ConfigureCustomer(modelBuilder);
        ConfigureVehicleMovement(modelBuilder);
        ConfigureUser(modelBuilder);

        // Seeding de dados iniciais
        SeedInitialData(modelBuilder);
    }

    private static void ConfigureVehicle(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.Vin).IsRequired().HasMaxLength(17);
            entity.HasIndex(v => v.Vin).IsUnique();
            entity.Property(v => v.Make).IsRequired().HasMaxLength(50);
            entity.Property(v => v.Model).IsRequired().HasMaxLength(50);
            entity.Property(v => v.Color).IsRequired().HasMaxLength(30);
            entity.Property(v => v.CostPrice).HasColumnType("decimal(18,2)");
            entity.Property(v => v.SellingPrice).HasColumnType("decimal(18,2)");

            // Relacionamentos
            entity.HasOne(v => v.Manufacturer)
                  .WithMany(m => m.Vehicles)
                  .HasForeignKey(v => v.ManufacturerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.Customer)
                  .WithMany(c => c.PurchasedVehicles)
                  .HasForeignKey(v => v.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureManufacturer(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Manufacturer>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Name).IsRequired().HasMaxLength(100);
            entity.Property(m => m.ContactName).HasMaxLength(100);
            entity.Property(m => m.Email).HasMaxLength(100);
            entity.Property(m => m.Phone).HasMaxLength(20);
            entity.Property(m => m.Address).HasMaxLength(200);
            entity.Property(m => m.Country).HasMaxLength(50);
        });
    }

    private static void ConfigureCustomer(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Email).HasMaxLength(100);
            entity.Property(c => c.Phone).HasMaxLength(20);
            entity.Property(c => c.Address).HasMaxLength(200);
            entity.Property(c => c.City).HasMaxLength(100);
            entity.Property(c => c.State).HasMaxLength(50);
            entity.Property(c => c.PostalCode).HasMaxLength(20);
            entity.Property(c => c.CpfCnpj).HasMaxLength(20);
        });
    }

    private static void ConfigureVehicleMovement(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VehicleMovement>(entity =>
        {
            entity.HasKey(vm => vm.Id);
            entity.Property(vm => vm.Description).IsRequired().HasMaxLength(200);
            entity.Property(vm => vm.Value).HasColumnType("decimal(18,2)");
            entity.Property(vm => vm.Notes).HasMaxLength(500);

            // Relacionamentos
            entity.HasOne(vm => vm.Vehicle)
                  .WithMany(v => v.Movements)
                  .HasForeignKey(vm => vm.VehicleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(vm => vm.Customer)
                  .WithMany(c => c.Movements)
                  .HasForeignKey(vm => vm.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(vm => vm.User)
                  .WithMany(u => u.Movements)
                  .HasForeignKey(vm => vm.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.UserName).IsRequired().HasMaxLength(50);
            entity.HasIndex(u => u.UserName).IsUnique();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(50);
        });
    }

    private static void SeedInitialData(ModelBuilder modelBuilder)
    {
        // Seed Manufacturers
        modelBuilder.Entity<Manufacturer>().HasData(
            new Manufacturer { Id = 1, Name = "Ford", ContactName = "Ford Brasil", Email = "contato@ford.com.br", Country = "Brasil", IsActive = true },
            new Manufacturer { Id = 2, Name = "Chevrolet", ContactName = "Chevrolet Brasil", Email = "contato@chevrolet.com.br", Country = "Brasil", IsActive = true },
            new Manufacturer { Id = 3, Name = "Volkswagen", ContactName = "Volkswagen do Brasil", Email = "contato@vw.com.br", Country = "Brasil", IsActive = true },
            new Manufacturer { Id = 4, Name = "Fiat", ContactName = "Stellantis Brasil", Email = "contato@fiat.com.br", Country = "Brasil", IsActive = true },
            new Manufacturer { Id = 5, Name = "Toyota", ContactName = "Toyota do Brasil", Email = "contato@toyota.com.br", Country = "Brasil", IsActive = true }
        );
    }
}