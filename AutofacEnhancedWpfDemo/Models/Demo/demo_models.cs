using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutofacEnhancedWpfDemo.Models.Demo;

// ========== DEMO CUSTOMER WITH EXTENDED DATA ==========

public class DemoCustomer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public CustomerType Type { get; set; } = CustomerType.Individual;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public List<DemoAddress> Addresses { get; set; } = new();
    public List<DemoOrder> Orders { get; set; } = new();
}

public enum CustomerType
{
    Individual,
    Business
}

// ========== DEMO ADDRESS ==========
public class DemoAddress
{
    public int Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public AddressType Type { get; set; } = AddressType.Billing;
    
    public int CustomerId { get; set; }
    public DemoCustomer Customer { get; set; } = null!;
}

public enum AddressType
{
    Billing,
    Shipping,
    Both
}

// ========== DEMO PRODUCT WITH EXTENDED DATA ==========

public class DemoProduct
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public decimal Weight { get; set; }
    public string Unit { get; set; } = "pcs";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public int? CategoryId { get; set; }
    public DemoProductCategory? Category { get; set; }
    
    public List<DemoOrderItem> OrderItems { get; set; } = new();
}

// ========== DEMO PRODUCT CATEGORY ==========

public class DemoProductCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public List<DemoProduct> Products { get; set; } = new();
}

// ========== DEMO ORDER ==========

public class DemoOrder
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public int CustomerId { get; set; }
    public DemoCustomer Customer { get; set; } = null!;
    public List<DemoOrderItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Quantity * i.UnitPrice);
}

// ========== DEMO ORDER ITEM ==========

public class DemoOrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public DemoOrder Order { get; set; } = null!;
    public int ProductId { get; set; }
    public DemoProduct Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
