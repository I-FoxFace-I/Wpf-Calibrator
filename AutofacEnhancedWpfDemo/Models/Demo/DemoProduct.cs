namespace AutofacEnhancedWpfDemo.Models.Demo;

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
