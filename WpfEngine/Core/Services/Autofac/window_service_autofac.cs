using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WpfEngine.Core.Services;
using WpfEngine.Core.ViewModels;
using WpfEngine.Core.Views;
using WpfEngine.Core.Views.Windows;
using WpfEngine.Services.WindowTracking;

namespace AutofacEnhancedWpfDemo.Services.Autofac;

/// <summary>
/// Window management service using Autofac
/// Uses WeakReferences for tracking to allow GC cleanup
/// Supports multiple ways to close windows (by Id, VmKey, VM instance, Window instance)
/// </summary>
public class WindowService : IWindowService
{
    private readonly ILifetimeScope _scope;
    private readonly IViewLocatorService _viewLocator;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly ILogger<WindowService> _logger;

    // Track windows by WindowId (Guid)
    private readonly Dictionary<Guid, WindowInfo> _windowsByGuid = new();
    
    // Track windows by VmKey for fast VM-based lookup
    private readonly Dictionary<VmKey, Guid> _windowIdsByVmKey = new();

    // Track parent-child relationships
    private readonly Dictionary<Guid, List<Guid>> _childWindows = new();
    private readonly Dictionary<Guid, TaskCompletionSource<object?>> _dialogCompletionSources = new();

    public WindowService(
        ILifetimeScope scope,
        IViewLocatorService viewLocator,
        IViewModelFactory viewModelFactory,
        ILogger<WindowService> logger)
    {
        _scope = scope;
        _viewLocator = viewLocator;
        _viewModelFactory = viewModelFactory;
        _logger = logger;
    }

    // ========== EVENTS ==========

    public event EventHandler<WindowClosedEventArgs>? WindowClosed;

    // ========== OPEN WINDOW ==========

    public Guid OpenWindow<TViewModel>() where TViewModel : IViewModel
    {
        var windowId = Guid.NewGuid();
        var viewModelType = typeof(TViewModel);

        _logger.LogInformation("[WINDOW_SERVICE] Opening window for {ViewModelType} (WindowId: {WindowId})",
            viewModelType.Name, windowId);

        var viewModel = _viewModelFactory.Create<TViewModel>();
        var view = _viewLocator.ResolveView<TViewModel>();

        return OpenWindowInternal(windowId, viewModel, view, null);
    }

    public Guid OpenWindow<TViewModel, TOptions>(TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters
    {
        var windowId = Guid.NewGuid();
        var viewModelType = typeof(TViewModel);

        _logger.LogInformation("[WINDOW_SERVICE] Opening window for {ViewModelType} with options (WindowId: {WindowId}, CorrelationId: {CorrelationId})",
            viewModelType.Name, windowId, options.CorrelationId);

        var viewModel = _viewModelFactory.Create<TViewModel, TOptions>(options);
        var view = _viewLocator.ResolveView<TViewModel>();

        return OpenWindowInternal(windowId, viewModel, view, null);
    }

    // ========== OPEN CHILD WINDOW ==========

    public Guid OpenChildWindow<TViewModel>(Guid parentWindowId)
        where TViewModel : IViewModel
    {
        var windowId = Guid.NewGuid();
        var viewModelType = typeof(TViewModel);

        _logger.LogInformation("[WINDOW_SERVICE] Opening child window for {ViewModelType} (WindowId: {WindowId}, Parent: {ParentId})",
            viewModelType.Name, windowId, parentWindowId);

        CleanupDeadWindows();

        if (!_windowsByGuid.TryGetValue(parentWindowId, out var parentInfo) || !parentInfo.IsAlive)
        {
            var message = $"Parent window {parentWindowId} not found or no longer alive";
            _logger.LogError("[WINDOW_SERVICE] {Message}", message);
            throw new InvalidOperationException(message);
        }

        var viewModel = _viewModelFactory.Create<TViewModel>();
        var view = _viewLocator.ResolveView<TViewModel>();

        return OpenWindowInternal(windowId, viewModel, view, parentInfo);
    }

    public Guid OpenChildWindow<TViewModel, TOptions>(Guid parentWindowId, TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters
    {
        var windowId = Guid.NewGuid();
        var viewModelType = typeof(TViewModel);

        _logger.LogInformation("[WINDOW_SERVICE] Opening child window for {ViewModelType} with options (WindowId: {WindowId}, Parent: {ParentId}, CorrelationId: {CorrelationId})",
            viewModelType.Name, windowId, parentWindowId, options.CorrelationId);

        CleanupDeadWindows();

        if (!_windowsByGuid.TryGetValue(parentWindowId, out var parentInfo) || !parentInfo.IsAlive)
        {
            var message = $"Parent window {parentWindowId} not found or no longer alive";
            _logger.LogError("[WINDOW_SERVICE] {Message}", message);
            throw new InvalidOperationException(message);
        }

        var viewModel = _viewModelFactory.Create<TViewModel, TOptions>(options);
        var view = _viewLocator.ResolveView<TViewModel>();

        return OpenWindowInternal(windowId, viewModel, view, parentInfo);
    }

    // ========== CLOSE WINDOW ==========

    /// <summary>
    /// Closes window by its Guid
    /// </summary>
    public void Close(Guid windowId)
    {
        _logger.LogInformation("[WINDOW_SERVICE] Closing window by Id: {WindowId}", windowId);

        CleanupDeadWindows();

        if (!_windowsByGuid.TryGetValue(windowId, out var windowInfo))
        {
            _logger.LogWarning("[WINDOW_SERVICE] Window {WindowId} not found", windowId);
            return;
        }

        if (windowInfo.TryGetWindow(out var window))
        {
            CloseAllChildWindows(windowId);
            window.Close();
        }
        else
        {
            _logger.LogWarning("[WINDOW_SERVICE] Window {WindowId} is no longer alive", windowId);
            CleanupWindow(windowId);
        }
    }

    /// <summary>
    /// Closes window by VmKey
    /// </summary>
    public void Close(VmKey vmKey)
    {
        _logger.LogInformation("[WINDOW_SERVICE] Closing window by VmKey: {VmKey}", vmKey);

        CleanupDeadWindows();

        if (!_windowIdsByVmKey.TryGetValue(vmKey, out var windowId))
        {
            _logger.LogWarning("[WINDOW_SERVICE] Window with VmKey {VmKey} not found", vmKey);
            return;
        }

        Close(windowId);
    }

    /// <summary>
    /// Closes window by ViewModel instance
    /// </summary>
    public void Close<TViewModel>(TViewModel viewModel) where TViewModel : IViewModel
    {
        if (viewModel == null)
            throw new ArgumentNullException(nameof(viewModel));

        var vmKey = viewModel.GetVmKey();
        Close(vmKey);
    }

    /// <summary>
    /// Closes window by Window instance
    /// </summary>
    public void Close(Window window)
    {
        if (window == null)
            throw new ArgumentNullException(nameof(window));

        // Find window by WindowId property if it implements IWindowView
        if (window is IWindowView windowView)
        {
            Close(windowView.WindowId);
            return;
        }

        // Fallback: search by reference
        CleanupDeadWindows();

        var windowInfo = _windowsByGuid.Values
            .FirstOrDefault(info => info.TryGetWindow(out var w) && ReferenceEquals(w, window));

        if (windowInfo != null)
        {
            Close(windowInfo.WindowId);
        }
        else
        {
            _logger.LogWarning("[WINDOW_SERVICE] Window instance not found in tracking");
            window.Close(); // Close anyway
        }
    }

    /// <summary>
    /// Closes window by ViewModel type and Guid (backwards compatibility)
    /// </summary>
    public void CloseWindow<TViewModel>(Guid windowId) where TViewModel : IViewModel
    {
        _logger.LogInformation("[WINDOW_SERVICE] Closing window {ViewModelType} (WindowId: {WindowId})",
            typeof(TViewModel).Name, windowId);

        Close(windowId);
    }

    public void CloseAllChildWindows(Guid parentWindowId)
    {
        _logger.LogInformation("[WINDOW_SERVICE] Closing all child windows of {ParentId}", parentWindowId);

        CleanupDeadWindows();

        if (!_childWindows.TryGetValue(parentWindowId, out var children))
        {
            _logger.LogDebug("[WINDOW_SERVICE] No child windows found for {ParentId}", parentWindowId);
            return;
        }

        var childrenCopy = children.ToList();

        foreach (var childId in childrenCopy)
        {
            Close(childId);
        }
    }

    /// <summary>
    /// Closes all child windows of given Window instance
    /// </summary>
    public void CloseAllChildWindows(Window parentWindow)
    {
        if (parentWindow == null)
            throw new ArgumentNullException(nameof(parentWindow));

        if (parentWindow is IWindowView windowView)
        {
            CloseAllChildWindows(windowView.WindowId);
        }
        else
        {
            _logger.LogWarning("[WINDOW_SERVICE] Parent window does not implement IWindowView");
        }
    }

    // ========== INTERNAL METHODS ==========

    private Guid OpenWindowInternal(
        Guid windowId,
        IViewModel viewModel,
        IView view,
        WindowInfo? parentInfo)
    {
        view.DataContext = viewModel;

        var window = view as Window;
        if (window == null)
        {
            var message = $"View {view.GetType().Name} is not a Window";
            _logger.LogError("[WINDOW_SERVICE] {Message}", message);
            throw new InvalidOperationException(message);
        }

        // Set WindowId if window implements IWindowView
        if (window is IWindowView windowView && windowView.WindowId != windowId)
        {
            _logger.LogWarning("[WINDOW_SERVICE] Window {WindowType} has different WindowId", window.GetType().Name);
        }

        // Setup parent-child relationship
        if (parentInfo != null)
        {
            if (parentInfo.TryGetWindow(out var parentWindow))
            {
                window.Owner = parentWindow;

                if (!_childWindows.ContainsKey(parentInfo.WindowId))
                {
                    _childWindows[parentInfo.WindowId] = new List<Guid>();
                }
                _childWindows[parentInfo.WindowId].Add(windowId);

                _logger.LogDebug("[WINDOW_SERVICE] Set window {WindowId} as child of {ParentId}",
                    windowId, parentInfo.WindowId);
            }
        }

        // Create VmKey
        var vmKey = viewModel.GetVmKey();

        // Track window
        var windowInfo = new WindowInfo
        {
            WindowId = windowId,
            VmKey = vmKey,
            WindowRef = new WeakReference<Window>(window),
            ViewModelRef = new WeakReference<IViewModel>(viewModel), // Cast to IViewModel
            ParentWindowId = parentInfo?.WindowId
        };

        _windowsByGuid[windowId] = windowInfo;
        _windowIdsByVmKey[vmKey] = windowId;

        // Subscribe to Closed event
        window.Closed += (s, e) => OnWindowClosedInternal(windowId, vmKey, viewModel);

        // Initialize ViewModel if needed
        if (viewModel is IInitializable initializable)
        {
            _logger.LogDebug("[WINDOW_SERVICE] Initializing {ViewModelType}", viewModel.GetType().Name);
            _ = initializable.InitializeAsync();
        }

        window.Show();

        _logger.LogInformation("[WINDOW_SERVICE] Window opened successfully (WindowId: {WindowId}, VmKey: {VmKey})",
            windowId, vmKey);

        return windowId;
    }

    private void OnWindowClosedInternal(Guid windowId, VmKey vmKey, object viewModel)
    {
        _logger.LogInformation("[WINDOW_SERVICE] Window closed (WindowId: {WindowId}, VmKey: {VmKey})",
            windowId, vmKey);

        CleanupWindow(windowId);

        // Dispose ViewModel if needed
        if (viewModel is IDisposable disposable)
        {
            _logger.LogDebug("[WINDOW_SERVICE] Disposing ViewModel {ViewModelType}", viewModel.GetType().Name);
            disposable.Dispose();
        }

        WindowClosed?.Invoke(this, new WindowClosedEventArgs(windowId, vmKey.ViewModelType, viewModel));
    }

    private void CleanupWindow(Guid windowId)
    {
        if (_windowsByGuid.TryGetValue(windowId, out var windowInfo))
        {
            _windowsByGuid.Remove(windowId);
            _windowIdsByVmKey.Remove(windowInfo.VmKey);

            if (_childWindows.ContainsKey(windowId))
            {
                _childWindows.Remove(windowId);
            }

            // Remove from any parent's child list
            foreach (var kvp in _childWindows)
            {
                kvp.Value.Remove(windowId);
            }
        }
    }

    private void CleanupDeadWindows()
    {
        var deadWindows = _windowsByGuid
            .Where(kvp => !kvp.Value.IsAlive)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var windowId in deadWindows)
        {
            _logger.LogDebug("[WINDOW_SERVICE] Cleaning up dead window {WindowId}", windowId);
            CleanupWindow(windowId);
        }
    }

    public void CloseDialog<TViewModel, TResult>(Guid windowId, TResult result)
        where TViewModel : IDialogViewModel
        where TResult : IVmResult
    {
        var viewModelType = typeof(TViewModel);

        _logger.LogInformation("[WINDOW_SERVICE] Closing dialog {ViewModelType} with result (ID: {WindowId})",
            viewModelType.Name, windowId);

        if (!_windowsByGuid.TryGetValue(windowId, out var windowInfo))
        {
            _logger.LogWarning("[WINDOW_SERVICE] Dialog window {WindowId} not found", windowId);
            return;
        }

        // Set DialogResult on window
        if (windowInfo.TryGetViewModel(out var view) && view is IDialogView<TViewModel> dialogView)
        {
            dialogView.DialogResult = true;
        }

        // Complete the TaskCompletionSource if this is an async dialog
        if (_dialogCompletionSources.TryGetValue(windowId, out var tcs))
        {
            tcs.TrySetResult(result);
            _dialogCompletionSources.Remove(windowId);
        }

        if(view is Window window)
        {
            window.Close();
        }
    }

    public void CloseDialog<TViewModel>(Guid windowId) where TViewModel : IDialogViewModel
    {
        var viewModelType = typeof(TViewModel);

        _logger.LogInformation("[WINDOW_SERVICE] Closing dialog {ViewModelType} with result (ID: {WindowId})",
            viewModelType.Name, windowId);

        if (!_windowsByGuid.TryGetValue(windowId, out var windowInfo))
        {
            _logger.LogWarning("[WINDOW_SERVICE] Dialog window {WindowId} not found", windowId);
            return;
        }

        // Set DialogResult on window
        if (windowInfo.TryGetViewModel(out var view) && view is IDialogView<TViewModel> dialogView)
        {
            dialogView.DialogResult = true;
        }

        if (view is Window window)
        {
            window.Close();
        }
    }

    // ========== SESSION MANAGEMENT (Stub - use WindowServiceRefactored for full support) ==========

    public Guid CreateSession(string sessionName)
    {
        _logger.LogWarning("[WINDOW_SERVICE] Session support not available in this WindowService. Use WindowServiceRefactored instead.");
        throw new NotSupportedException("Session support requires WindowServiceRefactored");
    }

    public void CloseSession(Guid sessionId)
    {
        _logger.LogWarning("[WINDOW_SERVICE] Session support not available in this WindowService. Use WindowServiceRefactored instead.");
        throw new NotSupportedException("Session support requires WindowServiceRefactored");
    }

    public Guid OpenWindowInSession<TViewModel>(Guid sessionId) where TViewModel : IViewModel
    {
        _logger.LogWarning("[WINDOW_SERVICE] Session support not available in this WindowService. Use WindowServiceRefactored instead.");
        throw new NotSupportedException("Session support requires WindowServiceRefactored");
    }

    public Guid OpenWindowInSession<TViewModel, TOptions>(Guid sessionId, TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters
    {
        _logger.LogWarning("[WINDOW_SERVICE] Session support not available in this WindowService. Use WindowServiceRefactored instead.");
        throw new NotSupportedException("Session support requires WindowServiceRefactored");
    }
}
