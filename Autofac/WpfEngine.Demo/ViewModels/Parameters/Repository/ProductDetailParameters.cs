using WpfEngine.Data.Parameters;

namespace WpfEngine.Demo.ViewModels.Parameters.Repository;

/// <summary>
/// Parameters for ProductInfoViewModel
/// REFACTORED: Renamed from DemoProductInfoParams
/// </summary>
public record ProductDetailParameters : BaseModelParameters
{
    public int ProductId { get; init; }
}
