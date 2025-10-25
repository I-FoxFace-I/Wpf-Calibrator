using System.ComponentModel;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Core.Services;

/// <summary>
/// Service for ViewModel navigation within a window
/// Used for workflows and content switching (ContentControl binding)
/// </summary>
public interface INavigationService : INotifyPropertyChanged
{
    // ========== NAVIGATE ==========

    /// <summary>
    /// Navigates to ViewModel without parameters
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
    /// Current ViewModel being displayed (bind to this in ContentControl)
    /// Notifies PropertyChanged when changed
    /// </summary>
    object? CurrentViewModel { get; }

    /// <summary>
    /// Can navigate back
    /// </summary>
    bool CanNavigateBack { get; }

    // ========== HISTORY ==========

    /// <summary>
    /// Clears navigation history
    /// </summary>
    void ClearHistory();

    /// <summary>
    /// Gets navigation history depth
    /// </summary>
    int HistoryDepth { get; }

    // ========== WINDOW CONTROL ==========

    /// <summary>
    /// Requests the host window to close
    /// Host ViewModel should subscribe to this event
    /// </summary>
    event EventHandler<WindowCloseRequestedEventArgs>? WindowCloseRequested;

    /// <summary>
    /// Requests window close (called from child ViewModels)
    /// </summary>
    void RequestWindowClose(bool showConfirmation = false, string? confirmationMessage = null);
}
