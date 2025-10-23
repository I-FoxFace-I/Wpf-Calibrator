using Microsoft.EntityFrameworkCore;
using AutofacEnhancedWpfDemo.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AutofacEnhancedWpfDemo.Data;

/// <summary>
/// Application database context - SQLite
/// Used via IDbContextFactory for proper isolation
/// </summary>
public class AppDbContext : DbContext
{
    private readonly ILogger<AppDbContext>? _logger;

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    // Constructor for factory pattern

    public AppDbContext()
    {
        
    }

    public AppDbContext(DbContextOptions options)
        : base(options)
    {
    }
    
    // Constructor with logger (optional)
    //public AppDbContext(DbContextOptions<DbContext> options, ILogger<AppDbContext> logger)
    //    : base(options)
    //{
    //    _logger = logger;
    //    _logger?.LogDebug("DbContext created");
    //}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>()
            .HasMany(c => c.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId);

        modelBuilder.Entity<Product>()
            .HasMany(p => p.OrderItems)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId);
    }

    public override void Dispose()
    {
        _logger?.LogDebug("DbContext disposed");
        base.Dispose();
    }
    
    public override async ValueTask DisposeAsync()
    {
        _logger?.LogDebug("DbContext disposed async");
        await base.DisposeAsync();
    }
}
