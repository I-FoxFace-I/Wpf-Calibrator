using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using WpfEngine.Core.Services;
using WpfEngine.Core.ViewModels;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Core.Services;

/// <summary>
/// Navigation service for ViewModel navigation within a window
/// DI-agnostic implementation - uses IViewModelFactory
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly ILogger<NavigationService> _logger;
    private readonly Stack<object> _navigationHistory = new();

    private object? _currentViewModel;

    public NavigationService(
        IViewModelFactory viewModelFactory,
        ILogger<NavigationService> logger)
    {
        _viewModelFactory = viewModelFactory;
        _logger = logger;
    }

    // ========== EVENTS ==========

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<WindowCloseRequestedEventArgs>? WindowCloseRequested;

    // ========== PROPERTIES ==========

    public object? CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            if (_currentViewModel != value)
            {
                // Dispose previous ViewModel if it implements IDisposable
                if (_currentViewModel is IDisposable disposable)
                {
                    _logger.LogInformation("[NAVIGATION] Disposing previous ViewModel {Type}",
                        _currentViewModel.GetType().Name);
                    disposable.Dispose();
                }

                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));

                _logger.LogInformation("[NAVIGATION] Current ViewModel changed to {Type}",
                    _currentViewModel?.GetType().Name ?? "null");
            }
        }
    }

    public bool CanNavigateBack => _navigationHistory.Count > 0;

    public int HistoryDepth => _navigationHistory.Count;

    // ========== NAVIGATION ==========

    public async Task NavigateToAsync<TViewModel>() where TViewModel : IViewModel
    {
        _logger.LogInformation("[NAVIGATION] Navigating to {ViewModelType}", typeof(TViewModel).Name);

        // Push current to history
        if (CurrentViewModel != null)
        {
            _navigationHistory.Push(CurrentViewModel);
            _logger.LogDebug("[NAVIGATION] Pushed {Type} to history (depth: {Depth})",
                CurrentViewModel.GetType().Name, _navigationHistory.Count);
        }

        // Create new ViewModel
        var viewModel = _viewModelFactory.Create<TViewModel>();

        // Initialize if needed
        if (viewModel is IInitializable initializable)
        {
            _logger.LogDebug("[NAVIGATION] Initializing {ViewModelType}", typeof(TViewModel).Name);
            await initializable.InitializeAsync();
        }

        CurrentViewModel = viewModel;
        OnPropertyChanged(nameof(CanNavigateBack));
    }

    public async Task NavigateToAsync<TViewModel, TOptions>(TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters
    {
        _logger.LogInformation("[NAVIGATION] Navigating to {ViewModelType} with options (CorrelationId: {CorrelationId})",
            typeof(TViewModel).Name, options.CorrelationId);

        // Push current to history
        if (CurrentViewModel != null)
        {
            _navigationHistory.Push(CurrentViewModel);
            _logger.LogDebug("[NAVIGATION] Pushed {Type} to history (depth: {Depth})",
                CurrentViewModel.GetType().Name, _navigationHistory.Count);
        }

        // Create new ViewModel with options
        var viewModel = _viewModelFactory.Create<TViewModel, TOptions>(options);

        // Initialize with options (important for MSDI)
        if (viewModel is IInitializable<TOptions> initializable)
        {
            _logger.LogDebug("[NAVIGATION] Initializing {ViewModelType} with options", typeof(TViewModel).Name);
            await initializable.InitializeAsync(options);
        }
        else if (viewModel is IInitializable initializableSimple)
        {
            _logger.LogDebug("[NAVIGATION] Initializing {ViewModelType}", typeof(TViewModel).Name);
            await initializableSimple.InitializeAsync();
        }
        else
        {
            _logger.LogWarning("[NAVIGATION] {ViewModelType} does not implement IInitializable", typeof(TViewModel).Name);
        }

        CurrentViewModel = viewModel;
        OnPropertyChanged(nameof(CanNavigateBack));
        OnPropertyChanged(nameof(HistoryDepth));
    }

    public async Task NavigateBackAsync()
    {
        if (!CanNavigateBack)
        {
            _logger.LogWarning("[NAVIGATION] Cannot navigate back - history is empty");
            return;
        }

        _logger.LogInformation("[NAVIGATION] Navigating back");

        var previousViewModel = _navigationHistory.Pop();
        _logger.LogDebug("[NAVIGATION] Popped {Type} from history (depth: {Depth})",
            previousViewModel.GetType().Name, _navigationHistory.Count);

        // Re-initialize if needed
        if (previousViewModel is IInitializable initializable)
        {
            _logger.LogDebug("[NAVIGATION] Re-initializing {ViewModelType}", previousViewModel.GetType().Name);
            await initializable.InitializeAsync();
        }

        CurrentViewModel = previousViewModel;
        OnPropertyChanged(nameof(CanNavigateBack));
    }

    // ========== HISTORY MANAGEMENT ==========

    public void ClearHistory()
    {
        _logger.LogInformation("[NAVIGATION] Clearing history (depth: {Depth})", _navigationHistory.Count);

        // Dispose all ViewModels in history
        while (_navigationHistory.Count > 0)
        {
            var vm = _navigationHistory.Pop();
            if (vm is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        OnPropertyChanged(nameof(CanNavigateBack));
        OnPropertyChanged(nameof(HistoryDepth));
    }

    // ========== WINDOW CONTROL ==========

    public void RequestWindowClose(bool showConfirmation = false, string? confirmationMessage = null)
    {
        _logger.LogInformation("[NAVIGATION] Window close requested (confirmation: {ShowConfirmation})",
            showConfirmation);

        WindowCloseRequested?.Invoke(this, new WindowCloseRequestedEventArgs(showConfirmation, confirmationMessage));
    }

    // ========== HELPERS ==========

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
