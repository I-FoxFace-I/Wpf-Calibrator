using System.Collections.Generic;

namespace AutofacEnhancedWpfDemo.Models;

/// <summary>
/// Represents a customer entity
/// </summary>
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<Order> Orders { get; set; } = new();
}