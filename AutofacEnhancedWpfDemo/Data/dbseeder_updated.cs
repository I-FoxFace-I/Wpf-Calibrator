using System.Linq;
using AutofacEnhancedWpfDemo.Models;

namespace AutofacEnhancedWpfDemo.Data;

/// <summary>
/// Seeds initial data into the database
/// </summary>
public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        // Check if already seeded
        if (context.Customers.Any())
            return;

        // Seed Customers
        var customers = new[]
        {
            new Customer { Name = "John Doe", Email = "john@example.com" },
            new Customer { Name = "Jane Smith", Email = "jane@example.com" },
            new Customer { Name = "Bob Johnson", Email = "bob@example.com" },
            new Customer { Name = "Alice Williams", Email = "alice@example.com" },
            new Customer { Name = "Charlie Brown", Email = "charlie@example.com" }
        };
        context.Customers.AddRange(customers);

        // Seed Products
        var products = new[]
        {
            new Product { Name = "Laptop", Price = 999.99m, Stock = 10 },
            new Product { Name = "Mouse", Price = 29.99m, Stock = 50 },
            new Product { Name = "Keyboard", Price = 79.99m, Stock = 30 },
            new Product { Name = "Monitor", Price = 299.99m, Stock = 15 },
            new Product { Name = "Headphones", Price = 149.99m, Stock = 25 },
            new Product { Name = "Webcam", Price = 89.99m, Stock = 20 },
            new Product { Name = "USB Cable", Price = 9.99m, Stock = 100 },
            new Product { Name = "Desk Mat", Price = 24.99m, Stock = 40 }
        };
        context.Products.AddRange(products);

        context.SaveChanges();
    }
}
