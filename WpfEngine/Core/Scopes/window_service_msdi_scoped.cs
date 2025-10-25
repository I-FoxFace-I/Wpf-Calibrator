using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WpfEngine.Core.Scopes;
using WpfEngine.Core.Services;
using WpfEngine.Core.ViewModels;
using WpfEngine.Core.Views;
using WpfEngine.Core.Views.Windows.MicrosoftDI;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Services.MicrosoftDI.Scopes;

/// <summary>
/// Window service with hierarchical scope support for Microsoft DI
/// Each window has its own scope context
/// </summary>
public class WindowServiceScoped : IWindowService
{
    private readonly IScopeContext _rootScopeContext;
    private readonly IScopeContextFactory _scopeFactory;
    private readonly IViewLocatorService _viewLocator;
    private readonly IScopeModuleCollection _moduleCollection;
    private readonly ILogger<WindowServiceScoped> _logger;

    // Track windows: WindowId -> WindowInfo
    private readonly Dictionary<Guid, WindowInfo> _openWindows = new();

    // Track parent-child relationships
    private readonly Dictionary<Guid, List<Guid>> _childWindows = new();

    // Track dialog completion sources
    private readonly Dictionary<Guid, TaskCompletionSource<object?>> _dialogCompletionSources = new();

    public WindowServiceScoped(
        IScopeContext rootScopeContext,
        IScopeContextFactory scopeFactory,
        IViewLocatorService viewLocator,
        IScopeModuleCollection moduleCollection,
        ILogger<WindowServiceScoped> logger)
    {
        _rootScopeContext = rootScopeContext;
        _scopeFactory = scopeFactory;
        _viewLocator = viewLocator;
        _moduleCollection = moduleCollection;
        _logger = logger;
    }

    public event EventHandler<WindowClosedEventArgs>? WindowClosed;

    // ========== OPEN WINDOW ==========

    public Guid OpenWindow<TViewModel>() where TViewModel : class
    {
        var windowId = Guid.NewGuid();
        var viewModelType = typeof(TViewModel);

        _logger.LogInformation("[WINDOW_SERVICE_SCOPED] Opening window for {ViewModelType} (ID: {WindowId})",
            viewModelType.Name, windowId);

        // Create child scope for this window
        var windowScope = _rootScopeContext.CreateChild($"window-{viewModelType.Name}-{windowId}");

        // Notify modules about scope creation
        _moduleCollection.NotifyScopeCreated(windowScope);

        // Create ViewModel from window's scope
        var viewModel = windowScope.ServiceProvider.GetService(viewModelType)
            ?? throw new InvalidOperationException($"Failed to resolve {viewModelType.Name}");

        // Register ViewModel in scope
        windowScope.RegisterInstance(viewModel);

        // Create View
        var view = _viewLocator.ResolveView(viewModelType);

        return OpenWindowInternal(windowId, viewModelType, viewModel, view, windowScope, null, null);
    }

    public Guid OpenWindow<TViewModel, TOptions>(TOptions options)
        where TViewModel : class
        where TOptions : ViewModelOptions
    {
        var windowId = Guid.NewGuid();
        var viewModelType = typeof(TViewModel);

        _logger.LogInformation("[WINDOW_SERVICE_SCOPED] Opening window for {ViewModelType} with options (ID: {WindowId}, CorrelationId: {CorrelationId})",
            viewModelType.Name, windowId, options.CorrelationId);

        // Create child scope for this window
        var windowScope = _rootScopeContext.CreateChild($"window-{viewModelType.Name}-{windowId}");

        // Register options in scope
        windowScope.RegisterInstance(options);

        // Notify modules about scope creation
        _moduleCollection.NotifyScopeCreated(windowScope);

        // Create ViewModel from window's scope
        var viewModel = windowScope.ServiceProvider.GetService(viewModelType)
            ?? throw new InvalidOperationException($"Failed to resolve {viewModelType.Name}");

        // Register ViewModel in scope
        windowScope.RegisterInstance(viewModel);

        // Create View
        var view = _viewLocator.ResolveView(viewModelType);

        return OpenWindowInternal(windowId, viewModelType, viewModel, view, windowScope, null, options);
    }

    // ========== OPEN CHILD WINDOW ==========

    public Guid OpenChildWindow<TViewModel>(Guid parentWindowId)
        where TViewModel : class
    {
        var windowId = Guid.NewGuid();
        var viewModelType = typeof(TViewModel);

        _logger.LogInformation("[WINDOW_SERVICE_SCOPED] Opening child window for {ViewModelType} (ID: {WindowId}, Parent: {ParentId})",
            viewModelType.Name, windowId, parentWindowId);

        // Get parent window info
        if (!_openWindows.TryGetValue(parentWindowId, out var parentInfo))
        {
            throw new InvalidOperationException($"Parent window {parentWindowId} not found");
        }

        // Create child scope from parent's scope
        var childScope = parentInfo.WindowScope.CreateChild($"child-{viewModelType.Name}-{windowId}");

        // Notify modules
        _moduleCollection.NotifyScopeCreated(childScope);

        // Create ViewModel from child scope
        var viewModel = childScope.ServiceProvider.GetService(viewModelType)
            ?? throw new InvalidOperationException($"Failed to resolve {viewModelType.Name}");

        // Register ViewModel in scope
        childScope.RegisterInstance(viewModel);

        // Create View
        var view = _viewLocator.ResolveView(viewModelType);

        return OpenWindowInternal(windowId, viewModelType, viewModel, view, childScope, parentInfo, null);
    }

    public Guid OpenChildWindow<TViewModel, TOptions>(Guid parentWindowId, TOptions options)
        where TViewModel : class
        where TOptions : ViewModelOptions
    {
        var windowId = Guid.NewGuid();
        var viewModelType = typeof(TViewModel);

        _logger.LogInformation("[WINDOW_SERVICE_SCOPED] Opening child window for {ViewModelType} with options (ID: {WindowId}, Parent: {ParentId}, CorrelationId: {CorrelationId})",
            viewModelType.Name, windowId, parentWindowId, options.CorrelationId);

        // Get parent window info
        if (!_openWindows.TryGetValue(parentWindowId, out var parentInfo))
        {
            throw new InvalidOperationException($"Parent window {parentWindowId} not found");
        }

        // Create child scope from parent's scope
        var childScope = parentInfo.WindowScope.CreateChild($"child-{viewModelType.Name}-{windowId}");

        // Register options in scope
        childScope.RegisterInstance(options);

        // Notify modules
        _moduleCollection.NotifyScopeCreated(childScope);

        // Create ViewModel from child scope
        var viewModel = childScope.ServiceProvider.GetService(viewModelType)
            ?? throw new InvalidOperationException($"Failed to resolve {viewModelType.Name}");

        // Register ViewModel in scope
        childScope.RegisterInstance(viewModel);

        // Create View
        var view = _viewLocator.ResolveView(viewModelType);

        return OpenWindowInternal(windowId, viewModelType, viewModel, view, childScope, parentInfo, options);
    }

    // ========== CLOSE WINDOW ==========

    public void CloseWindow<TViewModel>(Guid windowId) where TViewModel : class
    {
        var viewModelType = typeof(TViewModel);

        _logger.LogInformation("[WINDOW_SERVICE_SCOPED] Closing window {ViewModelType} (ID: {WindowId})",
            viewModelType.Name, windowId);

        if (!_openWindows.TryGetValue(windowId, out var windowInfo))
        {
            _logger.LogWarning("[WINDOW_SERVICE_SCOPED] Window {WindowId} not found", windowId);
            return;
        }

        if (windowInfo.ViewModelType != viewModelType)
        {
            _logger.LogWarning("[WINDOW_SERVICE_SCOPED] Window {WindowId} has different ViewModel type", windowId);
            return;
        }

        CloseAllChildWindows(windowId);
        windowInfo.Window.Close();
    }

    public void CloseAllChildWindows(Guid parentWindowId)
    {
        _logger.LogInformation("[WINDOW_SERVICE_SCOPED] Closing all child windows of {ParentId}", parentWindowId);

        if (!_childWindows.TryGetValue(parentWindowId, out var children))
        {
            return;
        }

        var childrenCopy = children.ToList();
        foreach (var childId in childrenCopy)
        {
            if (_openWindows.TryGetValue(childId, out var childInfo))
            {
                childInfo.Window.Close();
            }
        }
    }

    // ========== INTERNAL ==========

    private Guid OpenWindowInternal(
        Guid windowId,
        Type viewModelType,
        object viewModel,
        IView view,
        IScopeContext windowScope,
        WindowInfo? parentInfo,
        ViewModelOptions? options)
    {
        // Set DataContext
        view.DataContext = viewModel;

        // Get Window
        var window = (view as IWindow)?.Window
            ?? throw new InvalidOperationException($"View {view.GetType().Name} is not a Window");

        // Setup parent-child
        if (parentInfo != null)
        {
            window.Owner = parentInfo.Window;

            if (!_childWindows.ContainsKey(parentInfo.WindowId))
            {
                _childWindows[parentInfo.WindowId] = new List<Guid>();
            }
            _childWindows[parentInfo.WindowId].Add(windowId);
        }

        // Track window
        var windowInfo = new WindowInfo
        {
            WindowId = windowId,
            ViewModelType = viewModelType,
            ViewModel = viewModel,
            Window = window,
            WindowScope = windowScope,
            ParentWindowId = parentInfo?.WindowId
        };

        _openWindows[windowId] = windowInfo;

        // Subscribe to Closed
        window.Closed += (s, e) => OnWindowClosedInternal(windowId, viewModelType, viewModel, windowScope);

        // Initialize ViewModel
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

        window.Show();

        _logger.LogInformation("[WINDOW_SERVICE_SCOPED] Window opened (ID: {WindowId}, Scope: {ScopeId})",
            windowId, windowScope.ScopeId);

        return windowId;
    }

    private void OnWindowClosedInternal(
        Guid windowId,
        Type viewModelType,
        object viewModel,
        IScopeContext windowScope)
    {
        _logger.LogInformation("[WINDOW_SERVICE_SCOPED] Window closed (ID: {WindowId})", windowId);

        // Remove tracking
        _openWindows.Remove(windowId);

        if (_childWindows.TryGetValue(windowId, out var children))
        {
            _childWindows.Remove(windowId);
        }

        foreach (var kvp in _childWindows)
        {
            kvp.Value.Remove(windowId);
        }

        // Notify modules before disposing scope
        _moduleCollection.NotifyScopeDisposed(windowScope);

        // Dispose scope (cascades to all children and scoped instances including ViewModel)
        windowScope.Dispose();

        // Raise event
        WindowClosed?.Invoke(this, new WindowClosedEventArgs(windowId, viewModelType, viewModel));
    }

    void IWindowService.CloseDialog<TViewModel, TResult>(Guid windowId, TResult result)
    {
        throw new NotImplementedException();
    }

    // ========== WINDOW INFO ==========

    private class WindowInfo
    {
        public Guid WindowId { get; init; }
        public Type ViewModelType { get; init; } = null!;
        public object ViewModel { get; init; } = null!;
        public Window Window { get; init; } = null!;
        public IScopeContext WindowScope { get; init; } = null!;
        public Guid? ParentWindowId { get; init; }
    }
}