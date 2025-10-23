using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AutofacEnhancedWpfDemo.Models;

/// <summary>
/// Represents a product entity
/// </summary>
public class Product
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new();
}