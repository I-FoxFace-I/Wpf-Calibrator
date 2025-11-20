using WpfEngine.Data.Abstract;

namespace WpfEngine.Data.Parameters;

/// <summary>
/// Base record for ViewModel parameters
/// Provides default CorrelationId generation
/// </summary>
public abstract record BaseModelParameters : IViewModelParameters
{
    /// <summary>
    /// Correlation ID for tracking related operations
    /// </summary>
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}

