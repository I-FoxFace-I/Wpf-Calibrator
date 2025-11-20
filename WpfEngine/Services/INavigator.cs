using System;
using System.ComponentModel;
using System.Threading.Tasks;
using WpfEngine.Data.Abstract;
using WpfEngine.Data.Content;
using WpfEngine.ViewModels;

namespace WpfEngine.Services;

/// <summary>
/// Navigator service - unified navigation and content management
/// Combines responsibilities of INavigationService and IContentManager
/// Resolves ViewModels, initializes them, and manages navigation stack
/// </summary>
public interface INavigator : INotifyPropertyChanged, IDisposable
{
    // ========== CURRENT STATE ==========
    
    /// <summary>
    /// Current ViewModel being displayed
    /// </summary>
    object? CurrentViewModel { get; }
    
    /// <summary>
    /// Can navigate back in history
    /// </summary>
    bool CanNavigateBack { get; }
    
    /// <summary>
    /// Number of items in navigation history
    /// </summary>
    int HistoryDepth { get; }
    
    /// <summary>
    /// Whether Navigator owns and disposes ViewModels
    /// Default: true (disposes on navigate away / ClearHistory)
    /// Set to false if ViewModels are cached/managed externally
    /// </summary>
    bool OwnsViewModels { get; set; }
    
    // ========== NAVIGATION ==========
    
    /// <summary>
    /// Navigate to a new ViewModel (resolves, initializes, and displays)
    /// </summary>
    Task NavigateToAsync<TViewModel>() where TViewModel : IViewModel;
    
    /// <summary>
    /// Navigate to a new ViewModel with parameters
    /// </summary>
    Task NavigateToAsync<TViewModel, TParameters>(TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters;
    
    /// <summary>
    /// Navigate back to previous ViewModel
    /// </summary>
    Task NavigateBackAsync();
    
    /// <summary>
    /// Navigate back to specific ViewModel type (if in history)
    /// Returns true if navigation was successful
    /// </summary>
    Task<bool> NavigateBackToAsync<TViewModel>() where TViewModel : IViewModel;
    
    // ========== HISTORY MANAGEMENT ==========
    
    /// <summary>
    /// Clear navigation history and dispose all ViewModels (if owned)
    /// </summary>
    void ClearHistory();
    
    /// <summary>
    /// Check if specific ViewModel type is in history
    /// </summary>
    bool IsInHistory<TViewModel>() where TViewModel : IViewModel;
    
    // ========== WINDOW/SHELL CONTROL ==========
    
    /// <summary>
    /// Sets the close handler (typically provided by Window/Shell)
    /// Handler should return true if close was executed
    /// </summary>
    void SetCloseHandler(Func<bool, string?, Task<bool>> closeHandler);

    /// <summary>
    /// Requests close using the event handler that is invoked
    /// from inner side of current navigation content.
    /// </summar>s
    event EventHandler<NavigatorCloseRequestedEventArgs> NavigatorCloseRequest;

    /// <summary>
    /// Requests close using the configured handler
    /// Returns true if close was executed
    /// </summary>
    Task RequestCloseAsync(bool showConfirmation = false, string? confirmationMessage = null);
}
