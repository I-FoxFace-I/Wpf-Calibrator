using WpfEngine.Data.Parameters;

namespace WpfEngine.Demo.ViewModels.Parameters;

/// <summary>
/// Parameters for CustomerDetailViewModel
/// REFACTORED: Inherits from ViewModelParameters base class
/// </summary>
public record CustomerDetailParameters : BaseModelParameters
{
    public int CustomerId { get; init; }
}
