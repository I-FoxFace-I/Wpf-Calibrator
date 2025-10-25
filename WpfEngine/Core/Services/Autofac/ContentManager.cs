using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using WpfEngine.Core.ViewModels;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Core.Services.Autofac;

/// <summary>
/// Content manager - handles content navigation within a shell
/// Resolves content ViewModels from its own scope (window scope)
/// Notifies about CurrentContent changes
/// </summary>
public class ContentManager : IContentManager
{
    private readonly ILifetimeScope _scope;
    private readonly ILogger<ContentManager> _logger;
    private readonly Stack<object> _navigationHistory = new();

    private object? _currentContent;

    public ContentManager(
        ILifetimeScope scope,
        ILogger<ContentManager> logger)
    {
        _scope = scope;
        _logger = logger;
        
        _logger.LogDebug("[CONTENT_MANAGER] Created for scope {ScopeTag}", 
            scope.Tag?.ToString() ?? "untagged");
    }

    // ========== EVENTS ==========

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<ShellCloseRequestedEventArgs>? ShellCloseRequested;

    // ========== PROPERTIES ==========

    public object? CurrentContent
    {
        get => _currentContent;
        private set
        {
            if (_currentContent != value)
            {
                // Dispose previous content if it implements IDisposable
                if (_currentContent is IDisposable disposable)
                {
                    _logger.LogInformation("[CONTENT_MANAGER] Disposing previous content {Type}",
                        _currentContent.GetType().Name);
                    disposable.Dispose();
                }

                _currentContent = value;
                OnPropertyChanged(nameof(CurrentContent));

                _logger.LogInformation("[CONTENT_MANAGER] Current content changed to {Type}",
                    _currentContent?.GetType().Name ?? "null");
            }
        }
    }

    public bool CanNavigateBack => _navigationHistory.Count > 0;

    public int HistoryDepth => _navigationHistory.Count;

    // ========== NAVIGATION ==========

    public async Task NavigateToAsync<TViewModel>() where TViewModel : IViewModel
    {
        _logger.LogInformation("[CONTENT_MANAGER] Navigating to {ViewModelType}", typeof(TViewModel).Name);

        // Push current to history
        if (CurrentContent != null)
        {
            _navigationHistory.Push(CurrentContent);
            _logger.LogDebug("[CONTENT_MANAGER] Pushed {Type} to history (depth: {Depth})",
                CurrentContent.GetType().Name, _navigationHistory.Count);
        }

        // Create new ViewModel from CONTENT SCOPE (window scope)
        var viewModel = _scope.Resolve<TViewModel>();

        // Initialize if needed
        if (viewModel is IInitializable initializable)
        {
            _logger.LogDebug("[CONTENT_MANAGER] Initializing {ViewModelType}", typeof(TViewModel).Name);
            await initializable.InitializeAsync();
        }

        CurrentContent = viewModel;
        OnPropertyChanged(nameof(CanNavigateBack));
        OnPropertyChanged(nameof(HistoryDepth));
    }

    public async Task NavigateToAsync<TViewModel, TOptions>(TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters
    {
        _logger.LogInformation("[CONTENT_MANAGER] Navigating to {ViewModelType} with options (CorrelationId: {CorrelationId})",
            typeof(TViewModel).Name, options.CorrelationId);

        // Push current to history
        if (CurrentContent != null)
        {
            _navigationHistory.Push(CurrentContent);
            _logger.LogDebug("[CONTENT_MANAGER] Pushed {Type} to history (depth: {Depth})",
                CurrentContent.GetType().Name, _navigationHistory.Count);
        }

        // Create new ViewModel with options from CONTENT SCOPE
        var viewModel = _scope.Resolve<TViewModel>(new TypedParameter(typeof(TOptions), options));

        // Initialize with options
        if (viewModel is IInitializable<TOptions> initializable)
        {
            _logger.LogDebug("[CONTENT_MANAGER] Initializing {ViewModelType} with options", typeof(TViewModel).Name);
            await initializable.InitializeAsync(options);
        }
        else if (viewModel is IInitializable initializableSimple)
        {
            _logger.LogDebug("[CONTENT_MANAGER] Initializing {ViewModelType}", typeof(TViewModel).Name);
            await initializableSimple.InitializeAsync();
        }

        CurrentContent = viewModel;
        OnPropertyChanged(nameof(CanNavigateBack));
        OnPropertyChanged(nameof(HistoryDepth));
    }

    public async Task NavigateBackAsync()
    {
        if (!CanNavigateBack)
        {
            _logger.LogWarning("[CONTENT_MANAGER] Cannot navigate back - history is empty");
            return;
        }

        _logger.LogInformation("[CONTENT_MANAGER] Navigating back");

        var previousViewModel = _navigationHistory.Pop();
        _logger.LogDebug("[CONTENT_MANAGER] Popped {Type} from history (depth: {Depth})",
            previousViewModel.GetType().Name, _navigationHistory.Count);

        // Re-initialize if needed
        if (previousViewModel is IInitializable initializable)
        {
            _logger.LogDebug("[CONTENT_MANAGER] Re-initializing {ViewModelType}", previousViewModel.GetType().Name);
            await initializable.InitializeAsync();
        }

        CurrentContent = previousViewModel;
        OnPropertyChanged(nameof(CanNavigateBack));
        OnPropertyChanged(nameof(HistoryDepth));
    }

    // ========== HISTORY MANAGEMENT ==========

    public void ClearHistory()
    {
        _logger.LogInformation("[CONTENT_MANAGER] Clearing history (depth: {Depth})", _navigationHistory.Count);

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

    // ========== SHELL CONTROL ==========

    public void RequestShellClose(bool showConfirmation = false, string? confirmationMessage = null)
    {
        _logger.LogInformation("[CONTENT_MANAGER] Shell close requested (confirmation: {ShowConfirmation})",
            showConfirmation);

        ShellCloseRequested?.Invoke(this, new ShellCloseRequestedEventArgs(showConfirmation, confirmationMessage));
    }

    // ========== HELPERS ==========

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

