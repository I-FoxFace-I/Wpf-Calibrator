using WpfEngine.Data.Parameters;

namespace WpfEngine.Demo.ViewModels.Parameters.Repository;


/// <summary>
/// Parameters for OrderDetailViewModel
/// REFACTORED: Renamed from OrderDetailParams
/// </summary>
public record OrderDetailParameters : BaseModelParameters
{
    public int OrderId { get; init; }
}
