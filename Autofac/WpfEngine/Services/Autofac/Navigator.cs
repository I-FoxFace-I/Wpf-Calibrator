using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WpfEngine.Abstract;
using WpfEngine.Data.Abstract;
using WpfEngine.Data.Content;
using WpfEngine.Services;
using WpfEngine.ViewModels;

namespace WpfEngine.Services.Autofac;

/// <summary>
/// Navigator using lightweight IContentFactory
/// Manages navigation history and activation itself
/// No dependency on full ContentManager features
/// 
/// Benefits:
/// - DI-agnostic (only depends on IContentFactory interface)
/// - Simpler implementation
/// - Navigator controls activation/deactivation
/// - Easy to swap DI containers (just implement IContentFactory)
/// </summary>
public class Navigator : INavigator, INotifyPropertyChanged, IDisposable
{
    // Disposal
    private bool _disposed;

    private readonly ILogger<Navigator> _logger;
    private readonly IContentManager _contentManager;

    // Current state
    private object? _currentViewModel;
    private NavigationEntry? _currentEntry;

    // Navigation history (stores metadata, not ViewModels)
    private readonly Stack<NavigationEntry> _history = new();

    public event EventHandler<NavigatorCloseRequestedEventArgs>? NavigatorCloseRequest;

    public Navigator(
        IContentManager contentFactory, 
        ILogger<Navigator> logger)
    {
        _contentManager = contentFactory ?? throw new ArgumentNullException(nameof(contentFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        OwnsViewModels = true;
        _logger.LogDebug("[NAVIGATOR] Service created");
    }

    // ========== INavigator Implementation ==========

    public object? CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            if (_currentViewModel != value)
            {
                var oldViewModel = _currentViewModel;
                _currentViewModel = value;

                // Handle activation/deactivation
                HandleViewModelTransition(oldViewModel, value);

                OnPropertyChanged();
            }
        }
    }
    public bool OwnsViewModels { get; set; }
    public int HistoryDepth => _history.Count;
    public bool CanNavigateBack => _history.Count > 0;

    // ========== Navigation Methods ==========

    public async Task NavigateToAsync<TViewModel>() 
        where TViewModel : IViewModel
    {
        await NavigateToInternalAsync<TViewModel>(null, CancellationToken.None);
    }

    public async Task NavigateToAsync<TViewModel, TParameters>(TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters
    {
        await NavigateToInternalAsync<TViewModel>(parameters, CancellationToken.None);
    }

    public async Task NavigateBackAsync()
    {
        if (!CanNavigateBack)
        {
            _logger.LogWarning("[NAVIGATOR] Cannot navigate back - history is empty");
            
            return;
        }

        _logger.LogInformation("[NAVIGATOR] Navigating back");

        // Pop previous entry from history
        var previousEntry = _history.Pop();

        // Recreate ViewModel from entry
        await RestoreNavigationEntryAsync(previousEntry);

        OnPropertyChanged(nameof(CanNavigateBack));
        OnPropertyChanged(nameof(HistoryDepth));
    }

    public async Task<bool> NavigateBackToAsync<TViewModel>() 
        where TViewModel : IViewModel
    {
        var targetType = typeof(TViewModel);

        if (!_history.Any(e => e.ViewModelType == targetType))
        {
            _logger.LogWarning(
                "[NAVIGATOR] Type {Type} not found in history", 
                targetType.Name);
            return false;
        }

        _logger.LogInformation("[NAVIGATOR] Navigating back to {Type}", targetType.Name);

        // Pop entries until we find target type
        NavigationEntry? targetEntry = null;
        
        while (_history.Count > 0)
        {
            var entry = _history.Pop();
            if (entry.ViewModelType == targetType)
            {
                targetEntry = entry;
                break;
            }
        }

        if (targetEntry != null)
        {
            await RestoreNavigationEntryAsync(targetEntry);
            OnPropertyChanged(nameof(CanNavigateBack));
            OnPropertyChanged(nameof(HistoryDepth));
            return true;
        }

        return false;
    }

    public void ClearHistory()
    {
        _logger.LogInformation(
            "[NAVIGATOR] Clearing navigation history ({Count} entries)", 
            _history.Count);
        
        _history.Clear();
        
        OnPropertyChanged(nameof(CanNavigateBack));
        OnPropertyChanged(nameof(HistoryDepth));
    }

    public bool IsInHistory<TViewModel>() where TViewModel : IViewModel
    {
        return _history.Any(e => e.ViewModelType == typeof(TViewModel));
    }

    public void SetCloseHandler(Func<bool, string?, Task<bool>> closeHandler)
    {
        //_closeHandler = closeHandler;
        _logger.LogDebug("[NAVIGATOR] Close handler set");
    }

    public async Task RequestCloseAsync(
        bool showConfirmation = false, 
        string? confirmationMessage = null)
    {
        await Task.CompletedTask;
        
        _logger.LogInformation("[NAVIGATOR] Close requested (ShowConfirmation: {ShowConfirmation})", showConfirmation);

        // Raise event for external handling
        NavigatorCloseRequest?.Invoke(this, new(showConfirmation, confirmationMessage));
    }

    // ========== Private Methods ==========

    private async Task NavigateToInternalAsync<TViewModel>(
        IViewModelParameters? parameters,
        CancellationToken cancellationToken) 
        where TViewModel : IViewModel
    {
        try
        {
            _logger.LogInformation("[NAVIGATOR] Navigating to {Type}", typeof(TViewModel).Name);

            // Save current to history before navigating
            SaveCurrentToHistory();

            // Create new ViewModel via factory
            var newViewModel = await _contentManager.CreateContentAsync<TViewModel>(parameters, cancellationToken);

            // Update current entry and ViewModel
            _currentEntry = new NavigationEntry(typeof(TViewModel), parameters);
            
            CurrentViewModel = newViewModel;

            _logger.LogInformation(
                "[NAVIGATOR] Navigation to {Type} completed", 
                typeof(TViewModel).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NAVIGATOR] Navigation to {Type} failed", typeof(TViewModel).Name);
            throw;
        }
        finally
        {
            OnPropertyChanged(nameof(CurrentViewModel));
            OnPropertyChanged(nameof(CanNavigateBack));
            OnPropertyChanged(nameof(HistoryDepth));
        }
    }

    private void SaveCurrentToHistory()
    {
        if (_currentEntry != null && _currentViewModel != null)
        {
            _history.Push(_currentEntry);
            _logger.LogDebug("[NAVIGATOR] Saved {Type} to history", _currentEntry.ViewModelType.Name);
        }
    }

    private async Task RestoreNavigationEntryAsync(NavigationEntry entry)
    {
        _logger.LogDebug("[NAVIGATOR] Restoring {Type} from history", entry.ViewModelType.Name);

        try
        {
            // Recreate ViewModel from entry
            var restoredViewModel = await _contentManager.CreateContentAsync(entry.ViewModelType, entry.Parameters);

            _currentEntry = entry;
            CurrentViewModel = restoredViewModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NAVIGATOR] Failed to restore {Type}", entry.ViewModelType.Name);
            throw;
        }
    }

    private void HandleViewModelTransition(object? oldViewModel, object? newViewModel)
    {
        try
        {
            // Deactivate old ViewModel if it supports IActivatable
            if (oldViewModel is IActivatable oldActivatable)
            {
                _logger.LogDebug("[NAVIGATOR] Deactivating {Type}", oldViewModel.GetType().Name);
                    
                oldActivatable.OnDeactivateAsync().Wait();
            }

            // Dispose old ViewModel if we own it
            if (OwnsViewModels && oldViewModel is IViewModel oldVm)
            {
                _logger.LogDebug("[NAVIGATOR] Disposing {Type}", oldViewModel.GetType().Name);
                    
                _contentManager.DisposeContentAsync(oldVm).Wait();
            }

            // Activate new ViewModel if it supports IActivatable
            if (newViewModel is IActivatable newActivatable)
            {
                _logger.LogDebug("[NAVIGATOR] Activating {Type}", newViewModel.GetType().Name);
                    
                newActivatable.OnActivateAsync().Wait();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NAVIGATOR] Error during ViewModel transition");
        }
    }

    // ========== INotifyPropertyChanged ==========

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // ========== IDisposable ==========

    protected void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_disposed) return;

            _logger.LogDebug("[NAVIGATOR] Disposing");

            // Dispose current ViewModel if owned
            if (OwnsViewModels && _currentViewModel is IViewModel currentVm)
            {
                try
                {
                    _contentManager.DisposeContentAsync(currentVm).Wait();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "[NAVIGATOR] Error disposing current ViewModel");
                }
            }

            // Clear history
            _history.Clear();
            _logger.LogInformation("[NAVIGATOR] Disposed");
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // ========== Inner Types ==========

    /// <summary>
    /// Navigation entry that stores metadata about a navigation
    /// Does NOT hold ViewModel instances to avoid memory leaks
    /// </summary>
    private record NavigationEntry(
        Type ViewModelType, 
        IViewModelParameters? Parameters);
}

