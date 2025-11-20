using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Services;

/// <summary>
/// Navigation service for managing ViewModels and their associated Windows
/// Supports modal dialogs with results and content switching within windows
/// </summary>
public interface IWindowNavigator
{
    /// <summary>
    /// Shows modal dialog and returns result
    /// </summary>
    Task<TResult?> ShowDialogAsync<TViewModel, TResult>(object? parameters = null) 
        where TViewModel : class;
    
    /// <summary>
    /// Shows non-modal window
    /// </summary>
    void ShowWindow<TViewModel>(object? parameters = null) 
        where TViewModel : class;
    
    /// <summary>
    /// Switches current window content to new ViewModel (for workflows)
    /// </summary>
    void NavigateTo<TViewModel>(object? parameters = null) 
        where TViewModel : class;
    
    /// <summary>
    /// Closes dialog with result (called from ViewModel)
    /// </summary>
    void CloseDialog<TViewModel>(object? result = null) 
        where TViewModel : class;
    
    /// <summary>
    /// Gets current ViewModel (for content switching scenarios)
    /// </summary>
    object? CurrentViewModel { get; }
}

/// <summary>
/// Navigator implementation with ViewModel-first approach
/// </summary>
public class WindowNavigator : IWindowNavigator
{
    private readonly ILifetimeScope _scope;
    private readonly IViewLocator _viewLocator;
    private readonly ILogger<WindowNavigator> _logger;
    
    private readonly Dictionary<Type, TaskCompletionSource<object?>> _pendingDialogs = new();
    private readonly Dictionary<Type, Window> _openWindows = new();
    
    private object? _currentViewModel;
    private Window? _currentWindow;
    
    public object? CurrentViewModel => _currentViewModel;
    
    public WindowNavigator(
        ILifetimeScope scope,
        IViewLocator viewLocator,
        ILogger<WindowNavigator> logger)
    {
        _scope = scope;
        _viewLocator = viewLocator;
        _logger = logger;
    }
    
    public async Task<TResult?> ShowDialogAsync<TViewModel, TResult>(object? parameters = null) 
        where TViewModel : class
    {
        _logger.LogInformation("Showing modal dialog for {ViewModel}", typeof(TViewModel).Name);
        
        var vmType = typeof(TViewModel);
        var tcs = new TaskCompletionSource<object?>();
        _pendingDialogs[vmType] = tcs;
        
        try
        {
            // Resolve ViewModel with parameters
            var viewModel = ResolveViewModel<TViewModel>(parameters);
            
            // Create window using ViewLocator
            var window = _viewLocator.CreateWindowForViewModel<TViewModel>();
            window.DataContext = viewModel;
            
            _openWindows[vmType] = window;
            
            // Cleanup on close
            window.Closed += (s, e) =>
            {
                _pendingDialogs.Remove(vmType);
                _openWindows.Remove(vmType);
                _logger.LogInformation("Dialog {ViewModel} closed", typeof(TViewModel).Name);
            };
            
            // Show modal
            window.ShowDialog();
            
            // Await result
            var result = await tcs.Task;
            return result is TResult typedResult ? typedResult : default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing dialog {ViewModel}", typeof(TViewModel).Name);
            _pendingDialogs.Remove(vmType);
            throw;
        }
    }
    
    public void ShowWindow<TViewModel>(object? parameters = null) 
        where TViewModel : class
    {
        _logger.LogInformation("Showing window for {ViewModel}", typeof(TViewModel).Name);
        
        var viewModel = ResolveViewModel<TViewModel>(parameters);
        var window = _viewLocator.CreateWindowForViewModel<TViewModel>();
        window.DataContext = viewModel;
        
        var vmType = typeof(TViewModel);
        _openWindows[vmType] = window;
        
        window.Closed += (s, e) =>
        {
            _openWindows.Remove(vmType);
            _logger.LogInformation("Window {ViewModel} closed", typeof(TViewModel).Name);
        };
        
        window.Show();
    }
    
    public void NavigateTo<TViewModel>(object? parameters = null) 
        where TViewModel : class
    {
        _logger.LogInformation("Navigating to {ViewModel}", typeof(TViewModel).Name);
        
        if (_currentWindow == null)
            throw new InvalidOperationException("No window context for navigation");
        
        var viewModel = ResolveViewModel<TViewModel>(parameters);
        _currentViewModel = viewModel;
        _currentWindow.DataContext = viewModel;
    }
    
    public void CloseDialog<TViewModel>(object? result = null) 
        where TViewModel : class
    {
        var vmType = typeof(TViewModel);
        
        _logger.LogInformation("Closing dialog {ViewModel} with result", vmType.Name);
        
        // Set result for awaiting caller
        if (_pendingDialogs.TryGetValue(vmType, out var tcs))
        {
            tcs.SetResult(result);
        }
        
        // Close physical window
        if (_openWindows.TryGetValue(vmType, out var window))
        {
            window.Close();
        }
    }
    
    internal void SetCurrentWindow(Window window)
    {
        _currentWindow = window;
    }
    
    private TViewModel ResolveViewModel<TViewModel>(object? parameters) 
        where TViewModel : class
    {
        if (parameters == null)
        {
            return _scope.Resolve<TViewModel>();
        }
        
        // Resolve with typed parameter
        return _scope.Resolve<TViewModel>(
            new TypedParameter(parameters.GetType(), parameters)
        );
    }
}
