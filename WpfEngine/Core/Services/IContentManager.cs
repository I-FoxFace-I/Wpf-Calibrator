using System;
using System.ComponentModel;
using System.Threading.Tasks;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Core.Services;

/// <summary>
/// Manages content navigation within a shell window
/// Similar to INavigationService but with explicit shell lifecycle
/// Each ShellViewModel has its own ContentManager instance
/// </summary>
public interface IContentManager : INotifyPropertyChanged
{
    // ========== NAVIGATE ==========

    /// <summary>
    /// Navigates to ViewModel without parameters
    /// Content is resolved from ContentManager's scope
    /// </summary>
    Task NavigateToAsync<TViewModel>() where TViewModel : IViewModel;

    /// <summary>
    /// Navigates to ViewModel with options
    /// </summary>
    Task NavigateToAsync<TViewModel, TOptions>(TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters;

    /// <summary>
    /// Navigates back to previous ViewModel
    /// </summary>
    Task NavigateBackAsync();

    // ========== STATE ==========

    /// <summary>
    /// Current content ViewModel (bind to this in ContentControl)
    /// Notifies PropertyChanged when changed
    /// </summary>
    object? CurrentContent { get; }

    /// <summary>
    /// Can navigate back
    /// </summary>
    bool CanNavigateBack { get; }

    /// <summary>
    /// Navigation history depth
    /// </summary>
    int HistoryDepth { get; }

    // ========== HISTORY ==========

    /// <summary>
    /// Clears navigation history
    /// </summary>
    void ClearHistory();

    // ========== SHELL LIFECYCLE ==========

    /// <summary>
    /// Event raised when content requests shell window closure
    /// ShellViewModel should handle this and close via WindowManager
    /// </summary>
    event EventHandler<ShellCloseRequestedEventArgs>? ShellCloseRequested;

    /// <summary>
    /// Requests the shell window to close
    /// Called from content ViewModels
    /// </summary>
    void RequestShellClose(bool showConfirmation = false, string? confirmationMessage = null);
}

/// <summary>
/// Event args for shell close request from content
/// </summary>
public class ShellCloseRequestedEventArgs : EventArgs
{
    public bool ShowConfirmation { get; init; }
    public string? ConfirmationMessage { get; init; }
    
    public ShellCloseRequestedEventArgs(bool showConfirmation = false, string? confirmationMessage = null)
    {
        ShowConfirmation = showConfirmation;
        ConfirmationMessage = confirmationMessage;
    }
}

