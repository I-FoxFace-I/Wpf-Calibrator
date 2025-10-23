using System;
using System.Collections.Generic;
using System.Linq;

namespace AutofacEnhancedWpfDemo.Models;

/// <summary>
/// Represents an order entity
/// </summary>
public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Quantity * i.UnitPrice);
}