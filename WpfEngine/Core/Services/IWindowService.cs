using System;
using System.Windows;
using WpfEngine.Core.ViewModels;
using WpfEngine.Services.WindowTracking;

namespace WpfEngine.Core.Services;

/// <summary>
/// Service for managing physical windows with hierarchical scope support
/// </summary>
public interface IWindowService
{
    // ========== SESSION MANAGEMENT ==========
    
    /// <summary>
    /// Creates a new session scope for sharing services across multiple windows
    /// </summary>
    Guid CreateSession(string sessionName);
    
    /// <summary>
    /// Closes session and all its windows
    /// </summary>
    void CloseSession(Guid sessionId);

    // ========== OPEN WINDOW ==========

    /// <summary>
    /// Opens new window for ViewModel without parameters
    /// </summary>
    Guid OpenWindow<TViewModel>() where TViewModel : IViewModel;

    /// <summary>
    /// Opens new window for ViewModel with options
    /// </summary>
    Guid OpenWindow<TViewModel, TOptions>(TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters;

    // ========== OPEN WINDOW IN SESSION ==========
    
    /// <summary>
    /// Opens window within specific session scope
    /// </summary>
    Guid OpenWindowInSession<TViewModel>(Guid sessionId) where TViewModel : IViewModel;
    
    /// <summary>
    /// Opens window within specific session scope with options
    /// </summary>
    Guid OpenWindowInSession<TViewModel, TOptions>(Guid sessionId, TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters;

    // ========== OPEN CHILD WINDOW ==========

    /// <summary>
    /// Opens child window attached to parent window
    /// Child closes automatically when parent closes
    /// </summary>
    Guid OpenChildWindow<TViewModel>(Guid parentWindowId)
        where TViewModel : IViewModel;

    /// <summary>
    /// Opens child window with options
    /// </summary>
    Guid OpenChildWindow<TViewModel, TOptions>(Guid parentWindowId, TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters;

    // ========== CLOSE WINDOW/DIALOG ==========

    /// <summary>
    /// Closes window by VmKey
    /// </summary>
    void Close(VmKey vmKey);

    /// <summary>
    /// Closes window by its Guid
    /// </summary>
    void Close(Guid windowId);

    /// <summary>
    /// Closes window by Window instance
    /// </summary>
    void Close(Window window);

    /// <summary>
    /// Closes window by ViewModel type and window ID
    /// </summary>
    void CloseWindow<TViewModel>(Guid windowId) where TViewModel : IViewModel;

    /// <summary>
    /// Closes all child windows of parent
    /// </summary>
    void CloseAllChildWindows(Guid parentWindowId);

    /// <summary>
    /// Closes all child windows of given Window instance
    /// </summary>
    void CloseAllChildWindows(Window parentWindow);

    /// <summary>
    /// Closes dialog with result
    /// </summary>
    void CloseDialog<TViewModel, TResult>(Guid windowId, TResult result)
        where TViewModel : IDialogViewModel
        where TResult : IVmResult;

    /// <summary>
    /// Closes dialog with result
    /// </summary>
    void CloseDialog<TViewModel>(Guid windowId)
        where TViewModel : IDialogViewModel;
    
    // ========== EVENTS ==========

    /// <summary>
    /// Raised when window is closed
    /// Use for cleanup or notification in specific scenarios
    /// </summary>
    event EventHandler<WindowClosedEventArgs>? WindowClosed;
}
