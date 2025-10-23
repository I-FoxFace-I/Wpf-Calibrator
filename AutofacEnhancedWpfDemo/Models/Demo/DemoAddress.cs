namespace AutofacEnhancedWpfDemo.Models.Demo;

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

    public string FullAddress => $"{Street}, {City} {ZipCode}, {Country}";
}
