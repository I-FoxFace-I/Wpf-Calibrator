using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WpfEngine.Core.Services;
using WpfEngine.Core.ViewModels;
using WpfEngine.Core.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Services.MicrosoftDI;

/// <summary>
/// Window management service using Microsoft DI
/// Uses post-construction initialization for parameterized ViewModels
/// </summary>
public class WindowService : IWindowService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IViewLocatorService _viewLocator;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly ILogger<WindowService> _logger;

    // Track windows: WindowId -> WindowInfo
    private readonly Dictionary<Guid, WindowInfo> _openWindows = new();

    // Track parent-child relationships
    private readonly Dictionary<Guid, List<Guid>> _childWindows = new();

    // Track dialog completion sources
    private readonly Dictionary<Guid, TaskCompletionSource<object?>> _dialogCompletionSources = new();

    public WindowService(
        IServiceProvider serviceProvider,
        IViewLocatorService viewLocator,
        IViewModelFactory viewModelFactory,
        ILogger<WindowService> logger)
    {
        _serviceProvider = serviceProvider;
        _viewLocator = viewLocator;
        _viewModelFactory = viewModelFactory;
        _logger = logger;
    }

    // ========== EVENTS ==========

    public event EventHandler<WindowClosedEventArgs>? WindowClosed;

    // ========== OPEN WINDOW ==========

    public Guid OpenWindow<TViewModel>() where TViewModel : class
    {
        var windowId = Guid.NewGuid();
        var viewModelType = typeof(TViewModel);

        _logger.LogInformation("[WINDOW_SERVICE_MSDI] Opening window for {ViewModelType} (ID: {WindowId})",
            viewModelType.Name, windowId);

        // Create ViewModel from DI
        var viewModel = _viewModelFactory.Create<TViewModel>();

        // Create View from DI
        var view = _viewLocator.ResolveView<TViewModel>();

        // Setup and show (async initialization happens inside)
        return OpenWindowInternal(windowId, viewModelType, viewModel, view, null);
    }

    public Guid OpenWindow<TViewModel, TOptions>(TOptions options)
        where TViewModel : class
        where TOptions : ViewModelOptions
    {
        var windowId = Guid.NewGuid();
        var viewModelType = typeof(TViewModel);

        _logger.LogInformation("[WINDOW_SERVICE_MSDI] Opening window for {ViewModelType} with options (ID: {WindowId}, CorrelationId: {CorrelationId})",
            viewModelType.Name, windowId, options.CorrelationId);

        // Create ViewModel from DI (options set via reflection in factory)
        var viewModel = _viewModelFactory.Create<TViewModel, TOptions>(options);

        // Create View from DI
        var view = _viewLocator.ResolveView<TViewModel>();

        // Setup and show
        return OpenWindowInternal(windowId, viewModelType, viewModel, view, null, options);
    }

    // ========== OPEN CHILD WINDOW ==========

    public Guid OpenChildWindow<TViewModel>(Guid parentWindowId)
        where TViewModel : class
    {
        var windowId = Guid.NewGuid();
        var viewModelType = typeof(TViewModel);

        _logger.LogInformation("[WINDOW_SERVICE_MSDI] Opening child window for {ViewModelType} (ID: {WindowId}, Parent: {ParentId})",
            viewModelType.Name, windowId, parentWindowId);

        // Verify parent exists
        if (!_openWindows.TryGetValue(parentWindowId, out var parentInfo))
        {
            var message = $"Parent window {parentWindowId} not found";
            _logger.LogError("[WINDOW_SERVICE_MSDI] {Message}", message);
            throw new InvalidOperationException(message);
        }

        // Create ViewModel
        var viewModel = _viewModelFactory.Create<TViewModel>();

        // Create View
        var view = _viewLocator.ResolveView<TViewModel>();

        // Setup and show as child
        return OpenWindowInternal(windowId, viewModelType, viewModel, view, parentInfo);
    }

    public Guid OpenChildWindow<TViewModel, TOptions>(Guid parentWindowId, TOptions options)
        where TViewModel : class
        where TOptions : ViewModelOptions
    {
        var windowId = Guid.NewGuid();
        var viewModelType = typeof(TViewModel);

        _logger.LogInformation("[WINDOW_SERVICE_MSDI] Opening child window for {ViewModelType} with options (ID: {WindowId}, Parent: {ParentId}, CorrelationId: {CorrelationId})",
            viewModelType.Name, windowId, parentWindowId, options.CorrelationId);

        // Verify parent exists
        if (!_openWindows.TryGetValue(parentWindowId, out var parentInfo))
        {
            var message = $"Parent window {parentWindowId} not found";
            _logger.LogError("[WINDOW_SERVICE_MSDI] {Message}", message);
            throw new InvalidOperationException(message);
        }

        // Create ViewModel with options
        var viewModel = _viewModelFactory.Create<TViewModel, TOptions>(options);

        // Create View
        var view = _viewLocator.ResolveView<TViewModel>();

        // Setup and show as child
        return OpenWindowInternal(windowId, viewModelType, viewModel, view, parentInfo, options);
    }

    // ========== CLOSE WINDOW ==========

    public void CloseWindow<TViewModel>(Guid windowId) where TViewModel : class
    {
        var viewModelType = typeof(TViewModel);

        _logger.LogInformation("[WINDOW_SERVICE_MSDI] Closing window {ViewModelType} (ID: {WindowId})",
            viewModelType.Name, windowId);

        if (!_openWindows.TryGetValue(windowId, out var windowInfo))
        {
            _logger.LogWarning("[WINDOW_SERVICE_MSDI] Window {WindowId} not found", windowId);
            return;
        }

        // Verify ViewModel type matches
        if (windowInfo.ViewModelType != viewModelType)
        {
            _logger.LogWarning("[WINDOW_SERVICE_MSDI] Window {WindowId} has different ViewModel type: expected {Expected}, actual {Actual}",
                windowId, viewModelType.Name, windowInfo.ViewModelType.Name);
            return;
        }

        // Close all child windows first
        CloseAllChildWindows(windowId);

        // Close the window
        windowInfo.Window.Close();
    }

    public void CloseDialog<TViewModel, TResult>(Guid windowId, TResult result)
        where TViewModel : class, IDialogViewModel<TResult>
    {
        var viewModelType = typeof(TViewModel);

        _logger.LogInformation("[WINDOW_SERVICE_MSDI] Closing dialog {ViewModelType} with result (ID: {WindowId})",
            viewModelType.Name, windowId);

        if (!_openWindows.TryGetValue(windowId, out var windowInfo))
        {
            _logger.LogWarning("[WINDOW_SERVICE_MSDI] Dialog window {WindowId} not found", windowId);
            return;
        }

        // Set result in ViewModel
        if (windowInfo.ViewModel is IDialogViewModel<TResult> dialogViewModel)
        {
            var resultProperty = dialogViewModel.GetType().GetProperty(nameof(IDialogViewModel<TResult>.DialogResult));
            resultProperty?.SetValue(dialogViewModel, result);

            var completedProperty = dialogViewModel.GetType().GetProperty(nameof(IDialogViewModel<TResult>.IsCompleted));
            completedProperty?.SetValue(dialogViewModel, true);
        }

        // Set DialogResult on window
        if (windowInfo.Window is IDialogWindow dialogView)
        {
            dialogView.DialogResult = true;
        }

        // Complete the TaskCompletionSource
        if (_dialogCompletionSources.TryGetValue(windowId, out var tcs))
        {
            tcs.TrySetResult(result);
            _dialogCompletionSources.Remove(windowId);
        }

        // Close the window
        windowInfo.Window.Close();
    }

    public void CloseAllChildWindows(Guid parentWindowId)
    {
        _logger.LogInformation("[WINDOW_SERVICE_MSDI] Closing all child windows of {ParentId}", parentWindowId);

        if (!_childWindows.TryGetValue(parentWindowId, out var children))
        {
            _logger.LogDebug("[WINDOW_SERVICE_MSDI] No child windows found for {ParentId}", parentWindowId);
            return;
        }

        // Create copy to avoid modification during iteration
        var childrenCopy = children.ToList();

        foreach (var childId in childrenCopy)
        {
            if (_openWindows.TryGetValue(childId, out var childInfo))
            {
                _logger.LogDebug("[WINDOW_SERVICE_MSDI] Closing child window {ChildId}", childId);
                childInfo.Window.Close();
            }
        }
    }

    // ========== OPEN AS DIALOG ==========

    public async Task<TResult?> OpenDialogAsync<TViewModel, TResult>()
        where TViewModel : class, IDialogViewModel<TResult>
    {
        var windowId = Guid.NewGuid();
        var viewModelType = typeof(TViewModel);

        _logger.LogInformation("[WINDOW_SERVICE_MSDI] Opening dialog for {ViewModelType} (ID: {WindowId})",
            viewModelType.Name, windowId);

        // Create ViewModel from DI
        var viewModel = _viewModelFactory.Create<TViewModel>();

        // Create View from DI
        var view = _viewLocator.ResolveView<TViewModel>();

        // Create TaskCompletionSource for async result
        var tcs = new TaskCompletionSource<object?>();
        _dialogCompletionSources[windowId] = tcs;

        // Open as dialog
        OpenDialogInternal(windowId, viewModelType, viewModel, view, null, null);

        // Wait for dialog to complete
        var result = await tcs.Task;

        return result is TResult typedResult ? typedResult : default;
    }

    public async Task<TResult?> OpenDialogAsync<TViewModel, TOptions, TResult>(TOptions options)
        where TViewModel : class, IDialogViewModel<TOptions, TResult>
        where TOptions : ViewModelOptions
    {
        var windowId = Guid.NewGuid();
        var viewModelType = typeof(TViewModel);

        _logger.LogInformation("[WINDOW_SERVICE_MSDI] Opening dialog for {ViewModelType} with options (ID: {WindowId}, CorrelationId: {CorrelationId})",
            viewModelType.Name, windowId, options.CorrelationId);

        // Create ViewModel from DI (options set via factory)
        var viewModel = _viewModelFactory.Create<TViewModel, TOptions>(options);

        // Create View from DI
        var view = _viewLocator.ResolveView<TViewModel>();

        // Create TaskCompletionSource for async result
        var tcs = new TaskCompletionSource<object?>();
        _dialogCompletionSources[windowId] = tcs;

        // Open as dialog
        OpenDialogInternal(windowId, viewModelType, viewModel, view, null, options);

        // Wait for dialog to complete
        var result = await tcs.Task;

        return result is TResult typedResult ? typedResult : default;
    }

    // ========== QUERY ==========

    public bool IsWindowOpen(Guid windowId)
    {
        return _openWindows.ContainsKey(windowId);
    }

    public Guid? GetWindowId(object viewModel)
    {
        var windowInfo = _openWindows.Values.FirstOrDefault(w => w.ViewModel == viewModel);
        return windowInfo?.WindowId;
    }

    // ========== INTERNAL METHODS ==========

    private Guid OpenDialogInternal(
        Guid windowId,
        Type viewModelType,
        object viewModel,
        IView view,
        WindowInfo? parentInfo,
        ViewModelOptions? options)
    {
        // Set DataContext
        view.DataContext = viewModel;

        // Get Window
        var window = (view as IWindow)?.Window
            ?? throw new InvalidOperationException($"View {view.GetType().Name} is not a Window");

        // Set as dialog window
        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

        // Set owner
        if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsActive)
        {
            window.Owner = Application.Current.MainWindow;
        }
        else
        {
            foreach (Window w in Application.Current.Windows)
            {
                if (w.IsActive)
                {
                    window.Owner = w;
                    break;
                }
            }
        }

        // Track window
        var windowInfo = new WindowInfo
        {
            WindowId = windowId,
            ViewModelType = viewModelType,
            ViewModel = viewModel,
            Window = window,
            ParentWindowId = parentInfo?.WindowId,
            IsDialog = true
        };

        _openWindows[windowId] = windowInfo;

        // Subscribe to Closed
        window.Closed += (s, e) => OnDialogClosedInternal(windowId, viewModelType, viewModel);

        // Initialize ViewModel BEFORE showing dialog
        window.Loaded += async (s, e) =>
        {
            if (options != null && viewModel is IInitializable initWithOptions)
            {
                var initMethod = viewModel.GetType()
                    .GetMethod(nameof(IInitializable<ViewModelOptions>.InitializeAsync), new[] { options.GetType() });

                if (initMethod != null)
                {
                    await (Task)initMethod.Invoke(viewModel, new object[] { options })!;
                }
            }
            else if (viewModel is IInitializable init)
            {
                await init.InitializeAsync();
            }
        };

        // Show as modal dialog
        _logger.LogInformation("[WINDOW_SERVICE_MSDI] Showing modal dialog (ID: {WindowId})", windowId);
        window.ShowDialog();

        return windowId;
    }

    private Guid OpenWindowInternal(
        Guid windowId,
        Type viewModelType,
        object viewModel,
        IView view,
        WindowInfo? parentInfo,
        ViewModelOptions? options = null)
    {
        // Set ViewModel as DataContext
        view.DataContext = viewModel;

        // Get Window instance
        var window = (view as IWindow)?.Window;
        if (window == null)
        {
            var message = $"View {view.GetType().Name} is not a Window";
            _logger.LogError("[WINDOW_SERVICE_MSDI] {Message}", message);
            throw new InvalidOperationException(message);
        }

        // Setup parent-child relationship
        if (parentInfo != null)
        {
            window.Owner = parentInfo.Window;

            // Track child
            if (!_childWindows.ContainsKey(parentInfo.WindowId))
            {
                _childWindows[parentInfo.WindowId] = new List<Guid>();
            }
            _childWindows[parentInfo.WindowId].Add(windowId);

            _logger.LogDebug("[WINDOW_SERVICE_MSDI] Set window {WindowId} as child of {ParentId}",
                windowId, parentInfo.WindowId);
        }

        // Track window
        var windowInfo = new WindowInfo
        {
            WindowId = windowId,
            ViewModelType = viewModelType,
            ViewModel = viewModel,
            Window = window,
            ParentWindowId = parentInfo?.WindowId
        };

        _openWindows[windowId] = windowInfo;

        // Subscribe to Closed event
        window.Closed += (s, e) => OnWindowClosedInternal(windowId, viewModelType, viewModel);

        // Initialize ViewModel asynchronously AFTER window is shown
        // This is the key difference with MSDI - initialization happens post-construction
        window.Loaded += async (s, e) =>
        {
            if (options != null && viewModel is IInitializable initializableWithOptions)
            {
                _logger.LogDebug("[WINDOW_SERVICE_MSDI] Initializing {ViewModelType} with options", viewModelType.Name);

                // Call InitializeAsync with options
                var initMethod = viewModel.GetType()
                    .GetMethod(nameof(IInitializable<ViewModelOptions>.InitializeAsync), new[] { options.GetType() });

                if (initMethod != null)
                {
                    await (Task)initMethod.Invoke(viewModel, new object[] { options })!;
                }
            }
            else if (viewModel is IInitializable initializable)
            {
                _logger.LogDebug("[WINDOW_SERVICE_MSDI] Initializing {ViewModelType}", viewModelType.Name);
                await initializable.InitializeAsync();
            }
        };

        // Show window
        window.Show();

        _logger.LogInformation("[WINDOW_SERVICE_MSDI] Window opened successfully (ID: {WindowId}, Type: {ViewModelType})",
            windowId, viewModelType.Name);

        return windowId;
    }

    private void OnWindowClosedInternal(Guid windowId, Type viewModelType, object viewModel)
    {
        _logger.LogInformation("[WINDOW_SERVICE_MSDI] Window closed (ID: {WindowId}, Type: {ViewModelType})",
            windowId, viewModelType.Name);

        // Remove from tracking
        _openWindows.Remove(windowId);

        // Remove from parent's child list
        if (_childWindows.TryGetValue(windowId, out var children))
        {
            _childWindows.Remove(windowId);
        }

        // Remove from any parent's child list
        foreach (var kvp in _childWindows)
        {
            kvp.Value.Remove(windowId);
        }

        // NOTE: ViewModel disposal is handled by the window's scope, not here

        // Raise event
        WindowClosed?.Invoke(this, new WindowClosedEventArgs(windowId, viewModelType, viewModel));
    }

    private void OnDialogClosedInternal(Guid windowId, Type viewModelType, object viewModel)
    {
        _logger.LogInformation("[WINDOW_SERVICE_MSDI] Dialog closed (ID: {WindowId}, Type: {ViewModelType})",
            windowId, viewModelType.Name);

        // Remove from tracking
        _openWindows.Remove(windowId);

        // Complete TaskCompletionSource
        if (_dialogCompletionSources.TryGetValue(windowId, out var tcs))
        {
            var dialogResultProperty = viewModel.GetType().GetProperty("DialogResult");
            var dialogResult = dialogResultProperty?.GetValue(viewModel);

            if (dialogResult != null)
            {
                tcs.TrySetResult(dialogResult);
            }
            else
            {
                tcs.TrySetResult(null);
            }

            _dialogCompletionSources.Remove(windowId);
        }

        // NOTE: ViewModel disposal is handled by the window's scope, not here

        // Raise event
        WindowClosed?.Invoke(this, new WindowClosedEventArgs(windowId, viewModelType, viewModel));
    }

    // ========== WINDOW INFO ==========

    private class WindowInfo
    {
        public Guid WindowId { get; init; }
        public Type ViewModelType { get; init; } = null!;
        public object ViewModel { get; init; } = null!;
        public Window Window { get; init; } = null!;
        public Guid? ParentWindowId { get; init; }
        public bool IsDialog { get; init; }
    }
}