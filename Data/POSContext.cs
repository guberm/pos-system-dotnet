using Microsoft.EntityFrameworkCore;
using POSSystem.Models;

namespace POSSystem.Data;

/// <summary>
/// Entity Framework DbContext for the POS System
/// </summary>
public class POSContext : DbContext
{
    public POSContext(DbContextOptions<POSContext> options) : base(options)
    {
    }

    // DbSets for each entity
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entity relationships and constraints
        ConfigureProduct(modelBuilder);
        ConfigureCategory(modelBuilder);
        ConfigureCustomer(modelBuilder);
        ConfigureOrder(modelBuilder);
        ConfigureOrderItem(modelBuilder);

        // Seed initial data
        SeedData(modelBuilder);
    }

    private static void ConfigureProduct(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SKU).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => e.SKU).IsUnique();
            entity.HasOne(e => e.Category)
                  .WithMany(e => e.Products)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
        });
    }

    private static void ConfigureCustomer(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Ignore(e => e.FullName); // Computed property, not mapped to database
        });
    }

    private static void ConfigureOrder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasOne(e => e.Customer)
                  .WithMany(e => e.Orders)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureOrderItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.LineTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Order)
                  .WithMany(e => e.OrderItems)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany(e => e.OrderItems)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics", Description = "Electronic devices and accessories" },
            new Category { Id = 2, Name = "Clothing", Description = "Apparel and fashion items" },
            new Category { Id = 3, Name = "Food & Beverages", Description = "Food and drink products" }
        );

        // Seed Products
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Wireless Headphones", SKU = "WH001", Price = 99.99m, StockQuantity = 50, CategoryId = 1 },
            new Product { Id = 2, Name = "Smart Phone", SKU = "SP001", Price = 699.99m, StockQuantity = 25, CategoryId = 1 },
            new Product { Id = 3, Name = "T-Shirt", SKU = "TS001", Price = 19.99m, StockQuantity = 100, CategoryId = 2 },
            new Product { Id = 4, Name = "Coffee Beans", SKU = "CB001", Price = 12.99m, StockQuantity = 75, CategoryId = 3 }
        );
    }
}