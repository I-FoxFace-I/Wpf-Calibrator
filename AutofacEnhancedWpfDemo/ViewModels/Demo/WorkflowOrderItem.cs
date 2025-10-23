using CommunityToolkit.Mvvm.ComponentModel;

namespace AutofacEnhancedWpfDemo.ViewModels.Demo;

public partial class WorkflowOrderItem : ObservableObject
{
    [ObservableProperty]
    private int _productId;
    
    [ObservableProperty]
    private string? _productName;
    
    [ObservableProperty]
    private decimal _unitPrice;
    
    [ObservableProperty]
    private int _quantity;
    
    public decimal Total => UnitPrice * Quantity;
    
    partial void OnQuantityChanged(int value) => OnPropertyChanged(nameof(Total));
    partial void OnUnitPriceChanged(decimal value) => OnPropertyChanged(nameof(Total));
}
