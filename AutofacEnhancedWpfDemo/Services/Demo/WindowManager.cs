using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.Services.Demo;

/// <summary>
/// Window manager - handles physical window lifecycle
/// Tracks windows by (Guid, Type) for multiple instances
/// Fires events for window lifecycle
/// </summary>
public class WindowManager : IWindowManager
{
    private readonly ILifetimeScope _scope;
    private readonly IViewLocator _viewLocator;
    private readonly ILogger<WindowManager> _logger;
    
    // Track windows by unique ID
    private readonly Dictionary<Guid, WindowInfo> _openWindows = new();
    
    // Track child windows for cascade closing
    private readonly List<Guid> _childWindowIds = new();
    
    // Track pending dialogs
    private readonly Dictionary<Type, TaskCompletionSource<object?>> _pendingDialogs = new();
    
    // Events
    public event EventHandler<WindowEventArgs>? WindowClosed;
    public event EventHandler<WindowEventArgs>? WindowOpened;
    
    public WindowManager(
        ILifetimeScope scope,
        IViewLocator viewLocator,
        ILogger<WindowManager> logger)
    {
        _scope = scope;
        _viewLocator = viewLocator;
        _logger = logger;
    }
    
    public void ShowWindow<TViewModel>(Guid? windowId = null, object? parameters = null) where TViewModel : class
    {
        var id = windowId ?? Guid.NewGuid();
        var vmType = typeof(TViewModel);
        
        _logger.LogInformation("Opening window for {ViewModel} with ID {WindowId}", vmType.Name, id);
        
        var viewModel = ResolveViewModel<TViewModel>(parameters);
        var window = _viewLocator.CreateWindowForViewModel<TViewModel>();
        window.DataContext = viewModel;
        
        var windowInfo = new WindowInfo
        {
            WindowId = id,
            ViewModelType = vmType,
            ViewModel = viewModel,
            Window = window
        };
        
        _openWindows[id] = windowInfo;
        
        window.Closed += (s, e) => OnWindowClosedInternal(id, vmType, viewModel);
        
        window.Show();
        
        WindowOpened?.Invoke(this, new WindowEventArgs(id, vmType, viewModel));
    }
    
    public void ShowChildWindow<TViewModel>(Guid windowId, object? parameters = null) where TViewModel : class
    {
        var vmType = typeof(TViewModel);
        
        _logger.LogInformation("Opening child window for {ViewModel} with ID {WindowId}", vmType.Name, windowId);
        
        var viewModel = ResolveViewModel<TViewModel>(parameters);
        var window = _viewLocator.CreateWindowForViewModel<TViewModel>();
        window.DataContext = viewModel;
        
        // Set owner to current active window
        var activeWindow = GetActiveWindow();
        if (activeWindow != null)
        {
            window.Owner = activeWindow;
        }
        
        var windowInfo = new WindowInfo
        {
            WindowId = windowId,
            ViewModelType = vmType,
            ViewModel = viewModel,
            Window = window
        };
        
        _openWindows[windowId] = windowInfo;
        _childWindowIds.Add(windowId);
        
        window.Closed += (s, e) => OnWindowClosedInternal(windowId, vmType, viewModel);
        
        window.Show();
        
        WindowOpened?.Invoke(this, new WindowEventArgs(windowId, vmType, viewModel));
    }
    
    public async Task<TResult?> ShowDialogAsync<TViewModel, TResult>(object? parameters = null) 
        where TViewModel : class
    {
        _logger.LogInformation("Showing modal dialog for {ViewModel}", typeof(TViewModel).Name);
        
        var vmType = typeof(TViewModel);
        var tcs = new TaskCompletionSource<object?>();
        _pendingDialogs[vmType] = tcs;
        
        var windowId = Guid.NewGuid();
        
        try
        {
            var viewModel = ResolveViewModel<TViewModel>(parameters);
            var window = _viewLocator.CreateWindowForViewModel<TViewModel>();
            window.DataContext = viewModel;
            
            // Set owner to current active window
            var activeWindow = GetActiveWindow();
            if (activeWindow != null)
            {
                window.Owner = activeWindow;
            }
            
            var windowInfo = new WindowInfo
            {
                WindowId = windowId,
                ViewModelType = vmType,
                ViewModel = viewModel,
                Window = window
            };
            
            _openWindows[windowId] = windowInfo;
            
            window.Closed += (s, e) =>
            {
                _pendingDialogs.Remove(vmType);
                OnWindowClosedInternal(windowId, vmType, viewModel);
            };
            
            WindowOpened?.Invoke(this, new WindowEventArgs(windowId, vmType, viewModel));
            
            window.ShowDialog();
            
            var result = await tcs.Task;
            return result is TResult typedResult ? typedResult : default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing dialog {ViewModel}", typeof(TViewModel).Name);
            _pendingDialogs.Remove(vmType);
            _openWindows.Remove(windowId);
            throw;
        }
    }
    
    public void CloseWindow<TViewModel>(TViewModel viewModel) where TViewModel : class
    {
        if (viewModel == null)
        {
            _logger.LogWarning("Cannot close window - ViewModel is null");
            return;
        }
        
        // Find window by ViewModel instance
        var windowInfo = _openWindows.Values.FirstOrDefault(w => ReferenceEquals(w.ViewModel, viewModel));
        
        if (windowInfo != null)
        {
            _logger.LogInformation("Closing window {WindowId} for {ViewModel}", 
                windowInfo.WindowId, typeof(TViewModel).Name);
            
            windowInfo.Window.Close();
        }
        else
        {
            _logger.LogWarning("Window not found for ViewModel {ViewModel}", typeof(TViewModel).Name);
        }
    }
    
    public void CloseWindow(Guid windowId)
    {
        if (_openWindows.TryGetValue(windowId, out var windowInfo))
        {
            _logger.LogInformation("Closing window {WindowId}", windowId);
            windowInfo.Window.Close();
        }
        else
        {
            _logger.LogWarning("Window {WindowId} not found", windowId);
        }
    }
    
    public void CloseAllChildWindows()
    {
        _logger.LogInformation("Closing all {Count} child windows", _childWindowIds.Count);
        
        // Create copy to avoid modification during iteration
        var childIds = _childWindowIds.ToList();
        
        foreach (var childId in childIds)
        {
            CloseWindow(childId);
        }
        
        _childWindowIds.Clear();
    }
    
    public void CloseDialog<TViewModel>(object? result = null) where TViewModel : class
    {
        var vmType = typeof(TViewModel);
        
        _logger.LogInformation("Closing dialog {ViewModel} with result", vmType.Name);
        
        if (_pendingDialogs.TryGetValue(vmType, out var tcs))
        {
            tcs.SetResult(result);
        }
        
        // Find and close window
        var windowInfo = _openWindows.Values.FirstOrDefault(w => w.ViewModelType == vmType);
        if (windowInfo != null)
        {
            windowInfo.Window.Close();
        }
    }
    
    public Window? GetActiveWindow()
    {
        return System.Windows.Application.Current?.Windows
            .OfType<Window>()
            .FirstOrDefault(w => w.IsActive) 
            ?? System.Windows.Application.Current?.MainWindow;
    }
    
    public bool IsWindowOpen(Guid windowId)
    {
        return _openWindows.ContainsKey(windowId);
    }
    
    public object? GetViewModel(Guid windowId)
    {
        return _openWindows.TryGetValue(windowId, out var windowInfo) 
            ? windowInfo.ViewModel 
            : null;
    }
    
    private void OnWindowClosedInternal(Guid windowId, Type vmType, object? viewModel)
    {
        _logger.LogInformation("Window {WindowId} closed for {ViewModel}", windowId, vmType.Name);
        
        _openWindows.Remove(windowId);
        _childWindowIds.Remove(windowId);
        
        // Dispose ViewModel if it's disposable
        if (viewModel is IDisposable disposable)
        {
            _logger.LogInformation("Disposing ViewModel {ViewModel}", vmType.Name);
            disposable.Dispose();
        }
        
        // Fire event
        WindowClosed?.Invoke(this, new WindowEventArgs(windowId, vmType, viewModel));
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
    
    private class WindowInfo
    {
        public Guid WindowId { get; init; }
        public Type ViewModelType { get; init; } = null!;
        public object? ViewModel { get; init; }
        public Window Window { get; init; } = null!;
    }
}
