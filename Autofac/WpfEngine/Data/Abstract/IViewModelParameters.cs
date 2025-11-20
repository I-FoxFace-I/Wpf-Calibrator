namespace WpfEngine.Data.Abstract;

/// <summary>
/// Base interface for ViewModel parameters
/// Used when creating ViewModels with initialization data
/// </summary>
public interface IViewModelParameters
{
    /// <summary>
    /// Correlation ID for tracking related operations
    /// </summary>
    Guid CorrelationId { get; }
}

