namespace WpfEngine.Demo.Models;

// ========== DEMO ORDER ==========

public class DemoOrder
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public int CustomerId { get; set; }
    public DemoCustomer Customer { get; set; } = null!;

    public int? ShippingAddressId { get; set; }
    public DemoAddress? ShippingAddress { get; set; }

    public List<DemoOrderItem> Items { get; set; } = new();

    public decimal Subtotal => Items.Sum(i => i.Quantity * i.UnitPrice);
    public decimal Tax => Subtotal * 0.21m; // 21% VAT
    public decimal Total => Subtotal + Tax;

    public string CustomerName { get; set; } = string.Empty;
    public string ShippingAddressText { get; set; } = string.Empty;
}
