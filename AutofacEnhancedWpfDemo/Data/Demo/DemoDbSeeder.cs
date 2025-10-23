using System.Linq;
using AutofacEnhancedWpfDemo.Models.Demo;

namespace AutofacEnhancedWpfDemo.Data.Demo;

public static class DemoDbSeeder
{
    public static void Seed(DemoDbContext context)
    {
        if (context.Customers.Any())
            return;

        // Categories
        var categories = new[]
        {
            new DemoProductCategory { Name = "Electronics", Description = "Electronic devices and accessories" },
            new DemoProductCategory { Name = "Office", Description = "Office supplies and equipment" },
            new DemoProductCategory { Name = "Accessories", Description = "Computer accessories" }
        };
        context.Categories.AddRange(categories);
        context.SaveChanges();

        // Products with extended data
        var products = new[]
        {
            new DemoProduct 
            { 
                Name = "Laptop Pro 15", 
                Description = "High-performance laptop with 16GB RAM",
                Barcode = "LAP-001-2024",
                Price = 1299.99m, 
                Stock = 15,
                Weight = 2.1m,
                Unit = "pcs",
                CategoryId = categories[0].Id
            },
            new DemoProduct 
            { 
                Name = "Wireless Mouse", 
                Description = "Ergonomic wireless mouse with USB receiver",
                Barcode = "MOU-002-2024",
                Price = 29.99m, 
                Stock = 50,
                Weight = 0.12m,
                Unit = "pcs",
                CategoryId = categories[2].Id
            },
            new DemoProduct 
            { 
                Name = "Mechanical Keyboard", 
                Description = "RGB mechanical keyboard with blue switches",
                Barcode = "KEY-003-2024",
                Price = 89.99m, 
                Stock = 30,
                Weight = 0.95m,
                Unit = "pcs",
                CategoryId = categories[2].Id
            },
            new DemoProduct 
            { 
                Name = "4K Monitor 27\"", 
                Description = "Ultra HD 4K monitor with HDR support",
                Barcode = "MON-004-2024",
                Price = 399.99m, 
                Stock = 12,
                Weight = 6.5m,
                Unit = "pcs",
                CategoryId = categories[0].Id
            },
            new DemoProduct 
            { 
                Name = "Office Chair", 
                Description = "Ergonomic office chair with lumbar support",
                Barcode = "CHA-005-2024",
                Price = 249.99m, 
                Stock = 8,
                Weight = 18.5m,
                Unit = "pcs",
                CategoryId = categories[1].Id
            }
        };
        context.Products.AddRange(products);
        context.SaveChanges();

        // Customers with extended data and addresses
        var customer1 = new DemoCustomer
        {
            Name = "John Smith",
            Email = "john.smith@example.com",
            Phone = "+1-555-0101",
            CompanyName = "Tech Solutions Inc.",
            TaxId = "US123456789",
            Type = CustomerType.Business
        };

        customer1.Addresses.Add(new DemoAddress
        {
            Street = "123 Main Street",
            City = "New York",
            ZipCode = "10001",
            Country = "USA",
            Type = AddressType.Both
        });

        var customer2 = new DemoCustomer
        {
            Name = "Emma Johnson",
            Email = "emma.j@example.com",
            Phone = "+1-555-0202",
            Type = CustomerType.Individual
        };

        customer2.Addresses.Add(new DemoAddress
        {
            Street = "456 Oak Avenue",
            City = "Los Angeles",
            ZipCode = "90001",
            Country = "USA",
            Type = AddressType.Billing
        });

        customer2.Addresses.Add(new DemoAddress
        {
            Street = "789 Pine Road",
            City = "San Francisco",
            ZipCode = "94102",
            Country = "USA",
            Type = AddressType.Shipping
        });

        var customer3 = new DemoCustomer
        {
            Name = "Robert Williams",
            Email = "r.williams@example.com",
            Phone = "+1-555-0303",
            CompanyName = "Global Trading LLC",
            TaxId = "US987654321",
            Type = CustomerType.Business
        };

        customer3.Addresses.Add(new DemoAddress
        {
            Street = "321 Business Blvd",
            City = "Chicago",
            ZipCode = "60601",
            Country = "USA",
            Type = AddressType.Both
        });

        context.Customers.AddRange(customer1, customer2, customer3);
        context.SaveChanges();
    }
}
