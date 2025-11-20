using CommunityToolkit.Mvvm.ComponentModel;
using WpfEngine.Data.Parameters;

namespace WpfEngine.Demo.ViewModels.Parameters;


/// <summary>
/// Parameters for ProductDetailViewModel
/// REFACTORED: Renamed from ProductDetailOptions
/// </summary>
public record ProductCreateParameters : BaseModelParameters
{
    public int ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; } = decimal.Zero;
    public decimal Weight { get; init; } = decimal.Zero;
    public string Unit { get; init; } = "pcs";
}
