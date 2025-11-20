using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WpfEngine.Data.Abstract;
using WpfEngine.Data.Dialogs;
using WpfEngine.Data.Evaluation;
using WpfEngine.Data.Windows.Events;
using WpfEngine.ViewModels;

namespace WpfEngine.Services;

/// <summary>
/// Per-window child management service with error handling
/// Instance per window scope - manages child windows and lifecycle
/// </summary>
public interface IWindowContext : IDisposable
{
    // ========== IDENTIFICATION ==========

    /// <summary>
    /// This window's unique identifier
    /// </summary>
    Guid WindowId { get; }

    // ========== EVENTS ==========

    /// <summary>
    /// Raised when a child window is closed
    /// </summary>
    event EventHandler<ChildWindowClosedEventArgs>? ChildClosed;

    /// <summary>
    /// Raised when a window operation fails
    /// </summary>
    event EventHandler<WindowContextErrorEventArgs>? OperationError;

    // ========== CHILD WINDOW OPERATIONS WITH ERROR HANDLING ==========

    /// <summary>
    /// Opens a child window with error handling
    /// </summary>
    OperationResult<Guid> TryOpenWindow<TViewModel>() where TViewModel : IViewModel;

    /// <summary>
    /// Opens a child window with parameters and error handling
    /// </summary>
    OperationResult<Guid> TryOpenWindow<TViewModel, TParameters>(TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters;

    // ========== ORIGINAL METHODS (throw on error) ==========

    /// <summary>
    /// Opens a child window from this window (throws on error)
    /// </summary>
    Guid OpenWindow<TViewModel>() where TViewModel : IViewModel;

    /// <summary>
    /// Opens a child window with parameters (throws on error)
    /// </summary>
    Guid OpenWindow<TViewModel, TParameters>(TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters;

    // ========== WINDOW MANAGEMENT ==========

    /// <summary>
    /// Closes this window with error handling
    /// </summary>
    OperationResult TryCloseWindow(bool showConfirmation = false);

    /// <summary>
    /// Closes this window (throws on error)
    /// </summary>
    void CloseWindow();

    /// <summary>
    /// Closes all child windows with error handling
    /// </summary>
    OperationResult TryCloseAllChildWindows();

    /// <summary>
    /// Closes all child windows (throws on error)
    /// </summary>
    void CloseAllChildWindows();

    /// <summary>
    /// Closes specific child window with error handling
    /// </summary>
    OperationResult TryCloseChild(Guid childId);

    /// <summary>
    /// Closes specific child window (throws on error)
    /// </summary>
    void CloseChildWindow(Guid childId);

    // ========== SHOW DIALOG METHODS ==========

    /// <summary>
    /// Shows modal dialog and returns simple result (OK/Cancel)
    /// </summary>
    Task<DialogResult> ShowDialogAsync<TViewModel>()
         where TViewModel : class, IViewModel, IDialogViewModel;

    /// <summary>
    /// Shows modal dialog with parameters and returns simple result
    /// </summary>
    Task<DialogResult> ShowDialogAsync<TViewModel, TParameters>(TParameters parameters)
        where TViewModel : class, IViewModel, IDialogViewModel<TParameters>
        where TParameters : IViewModelParameters;

    /// <summary>
    /// Shows modal dialog and returns strongly-typed result
    /// </summary>
    Task<DialogResult<TResult>> ShowDialogAsync<TViewModel, TResult>()
        where TViewModel : class, IViewModel, IDialogViewModel, IResultDialogViewModel<TResult>
        where TResult : class;

    /// <summary>
    /// Shows modal dialog with parameters and returns strongly-typed result
    /// </summary>
    Task<DialogResult<TResult>> ShowDialogAsync<TViewModel, TParameters, TResult>(TParameters parameters)
        where TViewModel : class, IViewModel, IDialogViewModel<TParameters>, IResultDialogViewModel<TResult>
        where TParameters : IViewModelParameters
        where TResult : class;

    // ========== QUERIES ==========

    /// <summary>
    /// Gets all child window IDs
    /// </summary>
    IReadOnlyList<Guid> GetChildIds();

    /// <summary>
    /// Checks if a specific child window is open
    /// </summary>
    bool IsWindowOpen(Guid childId);

    /// <summary>
    /// Gets the number of open child windows
    /// </summary>
    int ChildWindowsCount { get; }
}
