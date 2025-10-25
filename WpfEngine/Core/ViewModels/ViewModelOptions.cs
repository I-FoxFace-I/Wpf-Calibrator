using System;

namespace WpfEngine.Core.ViewModels;

/// <summary>
/// Base record for all ViewModel parameters/options
/// Provides correlation tracking across async operations
/// </summary>
public abstract record ViewModelOptions : IVmParameters
{
    /// <summary>
    /// Correlation ID for tracking this ViewModel across operations
    /// </summary>
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}

// ========== COMMON PARAMETER TYPES ==========

/// <summary>
/// Options for entity detail/edit ViewModels
/// </summary>
public record EntityDetailOptions<TKey>(TKey EntityId, bool ReadOnly = false) : ViewModelOptions;

/// <summary>
/// Options for list ViewModels with optional filter
/// </summary>
public record ListOptions(string? FilterText = null, int? PageSize = null) : ViewModelOptions;

/// <summary>
/// Options for dialog ViewModels with data payload
/// </summary>
public record DialogOptions<TData>(TData Data, string? Title = null) : ViewModelOptions;

/// <summary>
/// Options for confirmation dialogs
/// </summary>
public record ConfirmationOptions(
    string Message,
    string? Title = null,
    string ConfirmText = "OK",
    string CancelText = "Cancel"
) : ViewModelOptions;

/// <summary>
/// Options for workflow ViewModels
/// </summary>
public record WorkflowOptions(Guid WorkflowId, object? InitialData = null) : ViewModelOptions;
