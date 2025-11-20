using WpfEngine.Data.Abstract;
using WpfEngine.Enums;

namespace WpfEngine.ViewModels;

// ========== DIALOG INTERFACES ==========

/// <summary>
/// ViewModel for dialog windows (no return value)
/// </summary>
public interface IDialogViewModel : IViewModel
{
    /// <summary>
    /// Indicates if dialog was completed successfully
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Indicates if dialog was cancelled
    /// </summary>
    bool IsCancelled { get; }

    /// <summary>
    /// Referencing Dialog window by ID
    /// </summary>
    Guid DialogId { get; }

    /// <summary>
    /// Defines the result status of dialog when is closed.
    /// </summary>
    DialogStatus Status { get; }

}

/// <summary>
/// Dialog ViewModel with parameters (no result)
/// </summary>
public interface IDialogViewModel<TParameter> : IViewModel<TParameter>, IDialogViewModel
    where TParameter : IViewModelParameters
{
}

/// <summary>
/// ViewModel for dialog windows (no return value)
/// </summary>
public interface IResultDialogViewModel<out TData> : IDialogViewModel
    where TData : class
{
    /// <summary>
    /// Dialog result (null = cancelled/closed without result)
    /// </summary>
    TData? ResultData { get; }
}

/// <summary>
/// ViewModel for dialog windows with typed result
/// </summary>
public interface IResultDialogViewModel<TParameter, TResult> : IViewModel<TParameter>, IResultDialogViewModel<TResult>
    where TParameter : IViewModelParameters
    where TResult : class
{

}
