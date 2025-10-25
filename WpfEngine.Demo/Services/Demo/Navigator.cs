using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Demo.Services;

/// <summary>
/// Navigator - handles ViewModel navigation within a window
/// Used for workflows and content switching
/// Notifies about CurrentViewModel changes
/// </summary>
public class Navigator : INavigator, INotifyPropertyChanged
{
    private readonly ILifetimeScope _scope;
    private readonly ILogger<Navigator> _logger;
    private readonly Stack<object> _navigationHistory = new();

    private object? _currentViewModel;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<WindowCloseRequestedEventArgs>? WindowCloseRequested;

    public object? CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            if (_currentViewModel != value)
            {
                if (_currentViewModel is IDisposable disposable)
                {
                    _logger.LogInformation("Disposing previous ViewModel {Type}", _currentViewModel.GetType().Name);
                    disposable.Dispose();
                }

                _currentViewModel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentViewModel)));
            }
        }
    }

    public bool CanNavigateBack => _navigationHistory.Count > 0;

    public Navigator(
        ILifetimeScope scope,
        ILogger<Navigator> logger)
    {
        _scope = scope;
        _logger = logger;
    }

    public async Task NavigateToAsync<TViewModel>(object? parameters = null) where TViewModel : class
    {
        _logger.LogInformation("Navigating to {ViewModel}", typeof(TViewModel).Name);

        if (_currentViewModel != null)
        {
            _navigationHistory.Push(_currentViewModel);
        }

        var viewModel = ResolveViewModel<TViewModel>(parameters);
        CurrentViewModel = viewModel;

        if (viewModel is IAsyncInitializable initializable)
        {
            await initializable.InitializeAsync();
        }

        _logger.LogInformation("Navigated to {ViewModel}", typeof(TViewModel).Name);
    }

    public async Task NavigateBackAsync()
    {
        if (!CanNavigateBack)
        {
            _logger.LogWarning("Cannot navigate back - no history");
            return;
        }

        _logger.LogInformation("Navigating back");

        var previousViewModel = _navigationHistory.Pop();
        CurrentViewModel = previousViewModel;

        if (previousViewModel is IAsyncInitializable initializable)
        {
            await initializable.InitializeAsync();
        }

        _logger.LogInformation("Navigated back to {ViewModel}", previousViewModel.GetType().Name);
    }

    public void ClearHistory()
    {
        _logger.LogInformation("Clearing navigation history");

        while (_navigationHistory.Count > 0)
        {
            var vm = _navigationHistory.Pop();
            if (vm is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public void RequestWindowClose(bool showConfirmation = false, string? confirmationMessage = null)
    {
        _logger.LogInformation("Requesting window close (showConfirmation: {ShowConfirmation})", showConfirmation);

        WindowCloseRequested?.Invoke(this, new WindowCloseRequestedEventArgs
        {
            ShowConfirmation = showConfirmation,
            ConfirmationMessage = confirmationMessage
        });
    }

    private TViewModel ResolveViewModel<TViewModel>(object? parameters) where TViewModel : class
    {
        if (parameters == null)
        {
            return _scope.Resolve<TViewModel>();
        }

        return _scope.Resolve<TViewModel>(
            new TypedParameter(parameters.GetType(), parameters)
        );
    }
}

/// <summary>
/// Interface for ViewModels that need async initialization
/// </summary>
public interface IAsyncInitializable
{
    Task InitializeAsync();
}