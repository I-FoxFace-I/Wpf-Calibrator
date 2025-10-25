using WpfEngine.Demo.Models;
using Microsoft.EntityFrameworkCore;

namespace WpfEngine.Demo.Data;

/// <summary>
/// Demo database context - separate from main app
/// Shows advanced patterns without affecting existing code
/// </summary>
public class DemoDbContext : DbContext
{
    public DbSet<DemoCustomer> Customers => Set<DemoCustomer>();
    public DbSet<DemoAddress> Addresses => Set<DemoAddress>();
    public DbSet<DemoProduct> Products => Set<DemoProduct>();
    public DbSet<DemoProductCategory> Categories => Set<DemoProductCategory>();
    public DbSet<DemoOrder> Orders => Set<DemoOrder>();
    public DbSet<DemoOrderItem> OrderItems => Set<DemoOrderItem>();

    public DemoDbContext(DbContextOptions<DemoDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DemoCustomer>().HasKey(e => e.Id);
        modelBuilder.Entity<DemoAddress>().HasKey(e => e.Id);
        modelBuilder.Entity<DemoProduct>().HasKey(e => e.Id);
        modelBuilder.Entity<DemoProductCategory>().HasKey(e => e.Id);
        modelBuilder.Entity<DemoOrder>().HasKey(e => e.Id);
        modelBuilder.Entity<DemoOrderItem>().HasKey(e => e.Id);

        // Enums as integers
        modelBuilder.Entity<DemoAddress>(builder =>
            builder.Property(x => x.Type).HasConversion<int>());
        modelBuilder.Entity<DemoCustomer>(builder =>
            builder.Property(x => x.Type).HasConversion<int>());
        modelBuilder.Entity<DemoOrder>(builder =>
            builder.Property(x => x.Status).HasConversion<int>());

        // Customer -> Addresses (one-to-many)
        modelBuilder.Entity<DemoCustomer>()
            .HasMany(c => c.Addresses)
            .WithOne(a => a.Customer)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Customer -> Orders (one-to-many)
        modelBuilder.Entity<DemoCustomer>()
            .HasMany(c => c.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Order -> ShippingAddress (optional many-to-one)
        modelBuilder.Entity<DemoOrder>()
            .HasOne(o => o.ShippingAddress)
            .WithMany()
            .HasForeignKey(o => o.ShippingAddressId)
            .OnDelete(DeleteBehavior.SetNull);

        // Category -> Products (one-to-many)
        modelBuilder.Entity<DemoProductCategory>()
            .HasMany(c => c.Products)
            .WithOne(p => p.Category)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Order -> OrderItems (one-to-many)
        modelBuilder.Entity<DemoOrder>()
            .HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Product -> OrderItems (one-to-many)
        modelBuilder.Entity<DemoProduct>()
            .HasMany(p => p.OrderItems)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}