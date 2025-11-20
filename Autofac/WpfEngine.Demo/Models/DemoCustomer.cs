namespace WpfEngine.Demo.Models;

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
