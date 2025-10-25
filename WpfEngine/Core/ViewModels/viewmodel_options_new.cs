using System;

namespace WpfEngine.Core.ViewModels;

public interface IVmParameters
{
    public Guid CorrelationId { get; }
}

public interface IVmResult
{
    public Guid Key { get; }
}


/// <summary>
/// Base record for all ViewModel parameters
/// Provides correlation tracking across async operations
/// </summary>
public abstract record ViewModelParameters
{
    /// <summary>
    /// Correlation ID for tracking this ViewModel across operations
    /// </summary>
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}

// ========== COMMON PARAMETER TYPES ==========

/// <summary>
/// Parameters for entity detail/edit ViewModels
/// </summary>
public record EntityDetailParameters<TKey>(TKey EntityId, bool ReadOnly = false) : ViewModelParameters;

/// <summary>
/// Parameters for list ViewModels with optional filter
/// </summary>
public record ListParameters(string? FilterText = null, int? PageSize = null) : ViewModelParameters;

/// <summary>
/// Parameters for dialog ViewModels with data payload
/// </summary>
public record DialogParameters<TData>(TData Data, string? Title = null) : ViewModelParameters;

/// <summary>
/// Parameters for confirmation dialogs
/// </summary>
public record ConfirmationParameters(
    string Message,
    string? Title = null,
    string ConfirmText = "OK",
    string CancelText = "Cancel"
) : ViewModelParameters;

/// <summary>
/// Parameters for workflow ViewModels
/// </summary>
public record WorkflowParameters(Guid WorkflowId, object? InitialData = null) : ViewModelParameters;
