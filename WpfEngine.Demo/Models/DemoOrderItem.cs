using System;
using System.Collections.Generic;
using System.Linq;

namespace WpfEngine.Demo.Models;

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
    public decimal Total => Quantity * UnitPrice;

    public string ProductName { get; set; } = string.Empty;
}