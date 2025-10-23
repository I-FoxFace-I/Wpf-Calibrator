using System;
using System.Threading.Tasks;
using System.Windows;

namespace AutofacEnhancedWpfDemo.Services.Demo;

/// <summary>
/// Event args for window lifecycle events
/// </summary>
public class WindowEventArgs : EventArgs
{
    public Guid WindowId { get; init; }
    public Type ViewModelType { get; init; }
    public object? ViewModel { get; init; }
    
    public WindowEventArgs(Guid windowId, Type viewModelType, object? viewModel = null)
    {
        WindowId = windowId;
        ViewModelType = viewModelType;
        ViewModel = viewModel;
    }
}

/// <summary>
/// Manages physical Windows (opening, closing, lifecycle)
/// Separate from INavigator which handles ViewModel navigation
/// </summary>
public interface IWindowManager
{
    /// <summary>
    /// Event fired when a window is closed
    /// ViewModels can subscribe to react to window closing
    /// </summary>
    event EventHandler<WindowEventArgs>? WindowClosed;
    
    /// <summary>
    /// Event fired when a window is opened
    /// </summary>
    event EventHandler<WindowEventArgs>? WindowOpened;
    
    /// <summary>
    /// Opens a new independent window with ViewModel
    /// </summary>
    /// <param name="windowId">Optional window ID for tracking (auto-generated if null)</param>
    /// <param name="parameters">Optional parameters for ViewModel constructor</param>
    void ShowWindow<TViewModel>(Guid? windowId = null, object? parameters = null) where TViewModel : class;
    
    /// <summary>
    /// Opens a non-modal child window
    /// Child windows are automatically closed when parent closes
    /// </summary>
    /// <param name="windowId">Window ID for tracking</param>
    /// <param name="parameters">Optional parameters for ViewModel constructor</param>
    void ShowChildWindow<TViewModel>(Guid windowId, object? parameters = null) where TViewModel : class;
    
    /// <summary>
    /// Opens a modal dialog and returns result
    /// </summary>
    Task<TResult?> ShowDialogAsync<TViewModel, TResult>(object? parameters = null) where TViewModel : class;
    
    /// <summary>
    /// Closes a specific window by ViewModel instance
    /// Called from ViewModel: _windowManager.CloseWindow(this)
    /// </summary>
    void CloseWindow<TViewModel>(TViewModel viewModel) where TViewModel : class;
    
    /// <summary>
    /// Closes a specific window by ID
    /// </summary>
    void CloseWindow(Guid windowId);
    
    /// <summary>
    /// Closes all child windows opened by current scope
    /// Called automatically when parent window closes
    /// </summary>
    void CloseAllChildWindows();
    
    /// <summary>
    /// Closes a dialog with result (called from ViewModel)
    /// </summary>
    void CloseDialog<TViewModel>(object? result = null) where TViewModel : class;
    
    /// <summary>
    /// Gets the current active window (for setting owner)
    /// </summary>
    Window? GetActiveWindow();
    
    /// <summary>
    /// Checks if window with given ID is currently open
    /// </summary>
    bool IsWindowOpen(Guid windowId);
    
    /// <summary>
    /// Gets ViewModel for a specific window ID
    /// </summary>
    object? GetViewModel(Guid windowId);
}
