using Autofac;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using WpfEngine.Data.Abstract;
using WpfEngine.Data.Evaluation;
using WpfEngine.Data.Windows;
using WpfEngine.Data.Windows.Events;
using WpfEngine.ViewModels;

namespace WpfEngine.Services;

// ========== EXTENDED WINDOW MANAGER INTERFACE ==========

/// <summary>
/// Application-level window management service with error handling
/// Singleton service responsible for global window operations
/// </summary>
public interface IWindowManager
{
    // ========== EVENTS ==========

    /// <summary>
    /// Raised when a window is opened
    /// </summary>
    event EventHandler<WindowOpenedEventArgs>? WindowOpened;

    /// <summary>
    /// Raised when a window is closed
    /// </summary>
    event EventHandler<WindowClosedEventArgs>? WindowClosed;

    /// <summary>
    /// Raised when a window operation fails
    /// </summary>
    event EventHandler<WindowErrorEventArgs>? WindowError;

    // ========== WINDOW OPERATIONS WITH ERROR HANDLING ==========

    /// <summary>
    /// Opens a new root window with error handling
    /// </summary>
    OperationResult<Guid> TryOpenWindow<TViewModel>() where TViewModel : IViewModel;

    /// <summary>
    /// Opens a new root window with parameters and error handling
    /// </summary>
    OperationResult<Guid> TryOpenWindow<TViewModel, TParameters>(TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters;

    /// <summary>
    /// Opens a child window with error handling
    /// </summary>
    OperationResult<Guid> TryOpenChildWindow<TViewModel>(Guid parentWindowId)
        where TViewModel : IViewModel;

    /// <summary>
    /// Opens a child window with parameters and error handling
    /// </summary>
    OperationResult<Guid> TryOpenChildWindow<TViewModel, TParameters>(Guid parentWindowId, TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters;

    // ========== ORIGINAL METHODS (throw on error) ==========

    /// <summary>
    /// Opens a new root window (throws on error)
    /// </summary>
    Guid OpenWindow<TViewModel>() where TViewModel : IViewModel;

    /// <summary>
    /// Opens a new root window with parameters (throws on error)
    /// </summary>
    Guid OpenWindow<TViewModel, TParameters>(TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters;

    /// <summary>
    /// Opens a child window (throws on error)
    /// </summary>
    Guid OpenChildWindow<TViewModel>(Guid parentWindowId)
        where TViewModel : IViewModel;

    /// <summary>
    /// Opens a child window with parameters (throws on error)
    /// </summary>
    Guid OpenChildWindow<TViewModel, TParameters>(Guid parentWindowId, TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters;

    /// <summary>
    /// Open window in specific session
    /// </summary>
    Guid OpenWindowInSession<TViewModel>(Guid sessionId) where TViewModel : IViewModel;

    /// <summary>
    /// Open window in session with parameters
    /// </summary>
    Guid OpenWindowInSession<TViewModel, TParameters>(Guid sessionId, TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters;

    // ========== WINDOW MANAGEMENT ==========

    /// <summary>
    /// Closes a window with error handling
    /// </summary>
    OperationResult TryCloseWindow(Guid windowId);

    /// <summary>
    /// Closes all child windows of a parent
    /// </summary>
    OperationResult TryCloseAllChildWindows(Guid parentWindowId);

    /// <summary>
    /// Try to close all windows in session
    /// </summary>
    OperationResult TryCloseAllSessionWindows(Guid sessionId);

    /// <summary>
    /// Closes a window (throws on error)
    /// </summary>
    void CloseWindow(Guid windowId);

    /// <summary>
    /// Closes all child windows of a parent (throws on error)
    /// </summary>
    void CloseAllChildWindows(Guid parentWindowId);

    /// <summary>
    /// Close all windows in session
    /// </summary>
    void CloseAllSessionWindows(Guid sessionId);

    // ========== QUERIES ==========

    /// <summary>
    /// Check if a window is open
    /// </summary>
    bool IsWindowOpen(Guid windowId);

    /// <summary>
    /// Get parent window ID if window is a child
    /// </summary>
    Guid? GetParentWindowId(Guid windowId);

    /// <summary>
    /// Get all open window IDs
    /// </summary>
    IReadOnlyList<Guid> GetOpenWindowIds();

    /// <summary>
    /// Get all windows in session (including children)
    /// </summary>
    IReadOnlyList<Guid> GetSessionWindows(Guid sessionId);

    /// <summary>
    /// Get all child window IDs for a parent
    /// </summary>
    IReadOnlyList<Guid> GetChildWindowIds(Guid parentWindowId);

    /// <summary>
    /// Get the ViewModel type for a window
    /// </summary>
    Type? GetViewModelType(Guid windowId);

    // ========== ERROR HANDLING ==========

    /// <summary>
    /// Shows error dialog for window operation failures
    /// </summary>
    void ShowWindowError(string message, Exception? exception = null);

    /// <summary>
    /// Gets last error for diagnostics
    /// </summary>
    WindowErrorInfo? GetLastError();

    /// <summary>
    /// Activates window (brings to front)
    /// </summary>
    bool Activate(Guid windowId);

}
