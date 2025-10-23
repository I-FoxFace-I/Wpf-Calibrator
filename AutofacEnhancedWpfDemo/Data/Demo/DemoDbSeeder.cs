using System;
using System.Linq;
using AutofacEnhancedWpfDemo.Models.Demo;

namespace AutofacEnhancedWpfDemo.Data.Demo;

/// <summary>
/// Seeds demo database with sample data
/// </summary>
public static class DemoDbSeeder
{
    public static void Seed(DemoDbContext context)
    {
        // Check if already seeded
        if (context.Customers.Any())
            return;

        // ========== CATEGORIES ==========

        var categories = new[]
        {
            new DemoProductCategory
            {
                Name = "Electronics",
                Description = "Electronic devices and accessories"
            },
            new DemoProductCategory
            {
                Name = "Furniture",
                Description = "Office and home furniture"
            },
            new DemoProductCategory
            {
                Name = "Accessories",
                Description = "Computer and office accessories"
            },
            new DemoProductCategory
            {
                Name = "Software",
                Description = "Software licenses and subscriptions"
            }
        };
        context.Categories.AddRange(categories);
        context.SaveChanges();

        // ========== PRODUCTS ==========

        var products = new[]
        {
            new DemoProduct
            {
                Name = "Dell XPS 15 Laptop",
                Description = "High-performance laptop with Intel i7, 16GB RAM, 512GB SSD",
                Barcode = "LAP-001-2024",
                Price = 1299.99m,
                Stock = 15,
                Weight = 2.1m,
                Unit = "pcs",
                CategoryId = categories[0].Id
            },
            new DemoProduct
            {
                Name = "Logitech MX Master 3",
                Description = "Wireless ergonomic mouse with precision scrolling",
                Barcode = "MOU-002-2024",
                Price = 99.99m,
                Stock = 45,
                Weight = 0.14m,
                Unit = "pcs",
                CategoryId = categories[2].Id
            },
            new DemoProduct
            {
                Name = "Mechanical Keyboard RGB",
                Description = "RGB mechanical keyboard with Cherry MX switches",
                Barcode = "KEY-003-2024",
                Price = 149.99m,
                Stock = 30,
                Weight = 0.95m,
                Unit = "pcs",
                CategoryId = categories[2].Id
            },
            new DemoProduct
            {
                Name = "Samsung 27\" 4K Monitor",
                Description = "Ultra HD 4K monitor with HDR support and 144Hz",
                Barcode = "MON-004-2024",
                Price = 449.99m,
                Stock = 12,
                Weight = 6.5m,
                Unit = "pcs",
                CategoryId = categories[0].Id
            },
            new DemoProduct
            {
                Name = "Herman Miller Aeron Chair",
                Description = "Premium ergonomic office chair with full adjustability",
                Barcode = "CHA-005-2024",
                Price = 1249.99m,
                Stock = 8,
                Weight = 21.5m,
                Unit = "pcs",
                CategoryId = categories[1].Id
            },
            new DemoProduct
            {
                Name = "Standing Desk Electric",
                Description = "Height-adjustable standing desk with memory presets",
                Barcode = "DSK-006-2024",
                Price = 599.99m,
                Stock = 10,
                Weight = 45.0m,
                Unit = "pcs",
                CategoryId = categories[1].Id
            },
            new DemoProduct
            {
                Name = "USB-C Docking Station",
                Description = "Multi-port docking station with 4K display support",
                Barcode = "DOC-007-2024",
                Price = 179.99m,
                Stock = 25,
                Weight = 0.45m,
                Unit = "pcs",
                CategoryId = categories[2].Id
            },
            new DemoProduct
            {
                Name = "Noise Cancelling Headphones",
                Description = "Premium wireless headphones with active noise cancellation",
                Barcode = "HEA-008-2024",
                Price = 299.99m,
                Stock = 20,
                Weight = 0.25m,
                Unit = "pcs",
                CategoryId = categories[0].Id
            },
            new DemoProduct
            {
                Name = "Webcam 4K Pro",
                Description = "Professional 4K webcam with auto-focus and HDR",
                Barcode = "CAM-009-2024",
                Price = 199.99m,
                Stock = 18,
                Weight = 0.18m,
                Unit = "pcs",
                CategoryId = categories[0].Id
            },
            new DemoProduct
            {
                Name = "Microsoft Office 365",
                Description = "Annual subscription for Office productivity suite",
                Barcode = "SFT-010-2024",
                Price = 89.99m,
                Stock = 100,
                Weight = 0.0m,
                Unit = "license",
                CategoryId = categories[3].Id
            },
            new DemoProduct
            {
                Name = "External SSD 1TB",
                Description = "Portable solid state drive with USB 3.2 Gen 2",
                Barcode = "SSD-011-2024",
                Price = 129.99m,
                Stock = 35,
                Weight = 0.08m,
                Unit = "pcs",
                CategoryId = categories[2].Id
            },
            new DemoProduct
            {
                Name = "LED Desk Lamp",
                Description = "Smart LED lamp with adjustable color temperature",
                Barcode = "LMP-012-2024",
                Price = 59.99m,
                Stock = 42,
                Weight = 0.65m,
                Unit = "pcs",
                CategoryId = categories[2].Id
            },
            new DemoProduct
            {
                Name = "Cable Management Kit",
                Description = "Complete cable management solution for office desks",
                Barcode = "CBL-013-2024",
                Price = 24.99m,
                Stock = 60,
                Weight = 0.35m,
                Unit = "set",
                CategoryId = categories[2].Id
            },
            new DemoProduct
            {
                Name = "Wireless Charger Pad",
                Description = "Fast wireless charging pad for smartphones",
                Barcode = "CHG-014-2024",
                Price = 39.99m,
                Stock = 50,
                Weight = 0.12m,
                Unit = "pcs",
                CategoryId = categories[2].Id
            },
            new DemoProduct
            {
                Name = "iPad Pro 12.9\"",
                Description = "Latest iPad Pro with M2 chip and 256GB storage",
                Barcode = "TAB-015-2024",
                Price = 1099.99m,
                Stock = 0, // Out of stock
                Weight = 0.68m,
                Unit = "pcs",
                CategoryId = categories[0].Id
            }
        };
        context.Products.AddRange(products);
        context.SaveChanges();

        // ========== CUSTOMERS WITH ADDRESSES ==========

        var customer1 = new DemoCustomer
        {
            Name = "John Smith",
            Email = "john.smith@techcorp.com",
            Phone = "+420 123 456 789",
            CompanyName = "TechCorp Solutions s.r.o.",
            TaxId = "CZ12345678",
            Type = CustomerType.Business,
            Addresses = new()
            {
                new DemoAddress
                {
                    Street = "Václavské námìstí 123",
                    City = "Praha",
                    ZipCode = "110 00",
                    Country = "Czech Republic",
                    Type = AddressType.Both
                },
                new DemoAddress
                {
                    Street = "Prùmyslová 45",
                    City = "Praha",
                    ZipCode = "150 00",
                    Country = "Czech Republic",
                    Type = AddressType.Shipping
                }
            }
        };

        var customer2 = new DemoCustomer
        {
            Name = "Jane Williams",
            Email = "jane.williams@gmail.com",
            Phone = "+420 234 567 890",
            CompanyName = "",
            TaxId = "",
            Type = CustomerType.Individual,
            Addresses = new()
            {
                new DemoAddress
                {
                    Street = "Karlova 28",
                    City = "Praha",
                    ZipCode = "110 00",
                    Country = "Czech Republic",
                    Type = AddressType.Both
                }
            }
        };

        var customer3 = new DemoCustomer
        {
            Name = "Robert Johnson",
            Email = "r.johnson@startup.io",
            Phone = "+420 345 678 901",
            CompanyName = "Startup Innovation Ltd.",
            TaxId = "CZ98765432",
            Type = CustomerType.Business,
            Addresses = new()
            {
                new DemoAddress
                {
                    Street = "Vinohradská 184",
                    City = "Praha",
                    ZipCode = "130 00",
                    Country = "Czech Republic",
                    Type = AddressType.Billing
                },
                new DemoAddress
                {
                    Street = "Nuselská 22",
                    City = "Praha",
                    ZipCode = "140 00",
                    Country = "Czech Republic",
                    Type = AddressType.Shipping
                }
            }
        };

        var customer4 = new DemoCustomer
        {
            Name = "Maria Garcia",
            Email = "maria.garcia@designstudio.cz",
            Phone = "+420 456 789 012",
            CompanyName = "Design Studio Prague",
            TaxId = "CZ55667788",
            Type = CustomerType.Business,
            Addresses = new()
            {
                new DemoAddress
                {
                    Street = "Paøížská 15",
                    City = "Praha",
                    ZipCode = "110 00",
                    Country = "Czech Republic",
                    Type = AddressType.Both
                }
            }
        };

        var customer5 = new DemoCustomer
        {
            Name = "David Brown",
            Email = "david.brown@personal.com",
            Phone = "+420 567 890 123",
            CompanyName = "",
            TaxId = "",
            Type = CustomerType.Individual,
            Addresses = new()
            {
                new DemoAddress
                {
                    Street = "Nové Sady 988/2",
                    City = "Brno",
                    ZipCode = "602 00",
                    Country = "Czech Republic",
                    Type = AddressType.Both
                },
                new DemoAddress
                {
                    Street = "Jugoslávská 24",
                    City = "Brno",
                    ZipCode = "613 00",
                    Country = "Czech Republic",
                    Type = AddressType.Shipping
                }
            }
        };

        context.Customers.AddRange(customer1, customer2, customer3, customer4, customer5);
        context.SaveChanges();

        // ========== SAMPLE ORDERS ==========

        var order1 = new DemoOrder
        {
            OrderNumber = "ORD-20250101-A1B2C3D4",
            OrderDate = new DateTime(2025, 1, 15, 10, 30, 0),
            CustomerId = customer1.Id,
            ShippingAddressId = customer1.Addresses.First(a => a.Type == AddressType.Shipping).Id,
            Status = OrderStatus.Delivered,
            Items = new()
            {
                new DemoOrderItem
                {
                    ProductId = products[0].Id, // Dell XPS Laptop
                    Quantity = 2,
                    UnitPrice = products[0].Price
                },
                new DemoOrderItem
                {
                    ProductId = products[3].Id, // Samsung Monitor
                    Quantity = 2,
                    UnitPrice = products[3].Price
                },
                new DemoOrderItem
                {
                    ProductId = products[6].Id, // USB-C Dock
                    Quantity = 2,
                    UnitPrice = products[6].Price
                }
            }
        };

        var order2 = new DemoOrder
        {
            OrderNumber = "ORD-20250110-E5F6G7H8",
            OrderDate = new DateTime(2025, 1, 20, 14, 15, 0),
            CustomerId = customer2.Id,
            ShippingAddressId = customer2.Addresses.First().Id,
            Status = OrderStatus.Shipped,
            Items = new()
            {
                new DemoOrderItem
                {
                    ProductId = products[1].Id, // Logitech Mouse
                    Quantity = 1,
                    UnitPrice = products[1].Price
                },
                new DemoOrderItem
                {
                    ProductId = products[2].Id, // Mechanical Keyboard
                    Quantity = 1,
                    UnitPrice = products[2].Price
                },
                new DemoOrderItem
                {
                    ProductId = products[11].Id, // LED Lamp
                    Quantity = 1,
                    UnitPrice = products[11].Price
                }
            }
        };

        var order3 = new DemoOrder
        {
            OrderNumber = "ORD-20250118-I9J0K1L2",
            OrderDate = new DateTime(2025, 1, 22, 9, 45, 0),
            CustomerId = customer3.Id,
            ShippingAddressId = customer3.Addresses.First(a => a.Type == AddressType.Shipping).Id,
            Status = OrderStatus.Processing,
            Items = new()
            {
                new DemoOrderItem
                {
                    ProductId = products[4].Id, // Herman Miller Chair
                    Quantity = 5,
                    UnitPrice = products[4].Price
                },
                new DemoOrderItem
                {
                    ProductId = products[5].Id, // Standing Desk
                    Quantity = 5,
                    UnitPrice = products[5].Price
                },
                new DemoOrderItem
                {
                    ProductId = products[9].Id, // Office 365
                    Quantity = 10,
                    UnitPrice = products[9].Price
                }
            }
        };

        var order4 = new DemoOrder
        {
            OrderNumber = "ORD-20250120-M3N4O5P6",
            OrderDate = new DateTime(2025, 1, 23, 16, 20, 0),
            CustomerId = customer4.Id,
            ShippingAddressId = customer4.Addresses.First().Id,
            Status = OrderStatus.Pending,
            Items = new()
            {
                new DemoOrderItem
                {
                    ProductId = products[7].Id, // Headphones
                    Quantity = 3,
                    UnitPrice = products[7].Price
                },
                new DemoOrderItem
                {
                    ProductId = products[8].Id, // Webcam
                    Quantity = 3,
                    UnitPrice = products[8].Price
                },
                new DemoOrderItem
                {
                    ProductId = products[10].Id, // External SSD
                    Quantity = 3,
                    UnitPrice = products[10].Price
                }
            }
        };

        context.Orders.AddRange(order1, order2, order3, order4);
        context.SaveChanges();
    }
}