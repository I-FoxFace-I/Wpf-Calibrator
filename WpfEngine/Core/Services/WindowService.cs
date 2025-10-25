using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WpfEngine.Core.ViewModels;
using WpfEngine.Core.Views;
using WpfEngine.Services.WindowTracking;

namespace WpfEngine.Core.Services.Autofac;

/// <summary>
/// Window management service using Autofac
/// Supports sessions for sharing services across multiple windows
/// Uses WeakReferences for tracking to allow GC cleanup
/// </summary>
public class WindowService : IWindowService
{
    private readonly ILifetimeScope _rootScope;
    private readonly IViewLocatorService _viewLocator;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly ILogger<WindowService> _logger;

    // Track windows by WindowId (Guid)
    private readonly Dictionary<Guid, WindowInfo> _windowsByGuid = new();
    
    // Track windows by VmKey for fast VM-based lookup
    private readonly Dictionary<VmKey, Guid> _windowIdsByVmKey = new();

    // Track parent-child relationships
    private readonly Dictionary<Guid, List<Guid>> _childWindows = new();
    
    // Track sessions
    private readonly Dictionary<Guid, SessionInfo> _sessions = new();
    private readonly Dictionary<Guid, Guid> _windowToSession = new(); // windowId -> sessionId

    public WindowService(
        ILifetimeScope rootScope,
        IViewLocatorService viewLocator,
        IViewModelFactory viewModelFactory,
        ILogger<WindowService> logger)
    {
        _rootScope = rootScope;
        _viewLocator = viewLocator;
        _viewModelFactory = viewModelFactory;
        _logger = logger;
    }

    // ========== EVENTS ==========

    public event EventHandler<WindowClosedEventArgs>? WindowClosed;

    // ========== SESSION MANAGEMENT ==========

    public Guid CreateSession(string? sessionTag = null)
    {
        var sessionId = Guid.NewGuid();
        var tag = sessionTag ?? $"session-{sessionId:N}";
        
        var sessionScope = _rootScope.BeginLifetimeScope(tag);
        
        _sessions[sessionId] = new SessionInfo
        {
            SessionId = sessionId,
            Tag = tag,
            Scope = sessionScope,
            WindowIds = new List<Guid>()
        };
        
        _logger.LogInformation("[SESSION] Created session {SessionId} with tag '{Tag}'", sessionId, tag);
        
        return sessionId;
    }

    public ILifetimeScope? GetSessionScope(Guid sessionId)
    {
        return _sessions.TryGetValue(sessionId, out var session) ? session.Scope : null;
    }

    public void CloseSession(Guid sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            _logger.LogWarning("[SESSION] Session {SessionId} not found", sessionId);
            return;
        }
        
        _logger.LogInformation("[SESSION] Closing session {SessionId} with {WindowCount} windows", 
            sessionId, session.WindowIds.Count);
        
        // Close all windows in session (copy list to avoid modification during iteration)
        foreach (var windowId in session.WindowIds.ToList())
        {
            Close(windowId);
        }
        
        // Dispose session scope
        session.Scope.Dispose();
        _sessions.Remove(sessionId);
        
        _logger.LogInformation("[SESSION] Session {SessionId} closed and disposed", sessionId);
    }

    // ========== OPEN WINDOW ==========

    public Guid OpenWindow<TViewModel>() where TViewModel : IViewModel
    {
        return OpenWindowInternal<TViewModel>(
            windowId: Guid.NewGuid(),
            parentScope: _rootScope,
            sessionId: null,
            parentWindowId: null,
            options: null);
    }

    public Guid OpenWindow<TViewModel, TOptions>(TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters
    {
        return OpenWindowInternal<TViewModel>(
            windowId: Guid.NewGuid(),
            parentScope: _rootScope,
            sessionId: null,
            parentWindowId: null,
            options: options);
    }

    // ========== OPEN WINDOW IN SESSION (INTERNAL USE) ==========

    internal Guid OpenWindowInSession<TViewModel>(Guid sessionId) where TViewModel : IViewModel
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            throw new InvalidOperationException($"Session {sessionId} not found");

        var windowId = Guid.NewGuid();
        session.WindowIds.Add(windowId);
        _windowToSession[windowId] = sessionId;

        return OpenWindowInternal<TViewModel>(
            windowId: windowId,
            parentScope: session.Scope,
            sessionId: sessionId,
            parentWindowId: null,
            options: null);
    }

    internal Guid OpenWindowInSession<TViewModel, TOptions>(Guid sessionId, TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            throw new InvalidOperationException($"Session {sessionId} not found");

        var windowId = Guid.NewGuid();
        session.WindowIds.Add(windowId);
        _windowToSession[windowId] = sessionId;

        return OpenWindowInternal<TViewModel>(
            windowId: windowId,
            parentScope: session.Scope,
            sessionId: sessionId,
            parentWindowId: null,
            options: options);
    }

    // ========== OPEN CHILD WINDOW ==========

    public Guid OpenChildWindow<TViewModel>(Guid parentWindowId)
        where TViewModel : IViewModel
    {
        CleanupDeadWindows();

        if (!_windowsByGuid.TryGetValue(parentWindowId, out var parentInfo) || !parentInfo.IsAlive)
        {
            var message = $"Parent window {parentWindowId} not found or no longer alive";
            _logger.LogError("[WINDOW_SERVICE] {Message}", message);
            throw new InvalidOperationException(message);
        }

        // Determine parent scope (session scope if parent is in session, otherwise root)
        ILifetimeScope parentScope = _rootScope;
        Guid? sessionId = null;

        if (_windowToSession.TryGetValue(parentWindowId, out var parentSessionId))
        {
            if (_sessions.TryGetValue(parentSessionId, out var session))
            {
                parentScope = session.Scope;
                sessionId = parentSessionId;
            }
        }

        var windowId = Guid.NewGuid();
        if (sessionId.HasValue)
        {
            _sessions[sessionId.Value].WindowIds.Add(windowId);
            _windowToSession[windowId] = sessionId.Value;
        }

        return OpenWindowInternal<TViewModel>(
            windowId: windowId,
            parentScope: parentScope,
            sessionId: sessionId,
            parentWindowId: parentWindowId,
            options: null);
    }

    public Guid OpenChildWindow<TViewModel, TOptions>(Guid parentWindowId, TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters
    {
        CleanupDeadWindows();

        if (!_windowsByGuid.TryGetValue(parentWindowId, out var parentInfo) || !parentInfo.IsAlive)
        {
            var message = $"Parent window {parentWindowId} not found or no longer alive";
            _logger.LogError("[WINDOW_SERVICE] {Message}", message);
            throw new InvalidOperationException(message);
        }

        // Determine parent scope (session scope if parent is in session, otherwise root)
        ILifetimeScope parentScope = _rootScope;
        Guid? sessionId = null;

        if (_windowToSession.TryGetValue(parentWindowId, out var parentSessionId))
        {
            if (_sessions.TryGetValue(parentSessionId, out var session))
            {
                parentScope = session.Scope;
                sessionId = parentSessionId;
            }
        }

        var windowId = Guid.NewGuid();
        if (sessionId.HasValue)
        {
            _sessions[sessionId.Value].WindowIds.Add(windowId);
            _windowToSession[windowId] = sessionId.Value;
        }

        return OpenWindowInternal<TViewModel>(
            windowId: windowId,
            parentScope: parentScope,
            sessionId: sessionId,
            parentWindowId: parentWindowId,
            options: options);
    }

    // ========== OPEN CHILD WINDOW IN SESSION (INTERNAL USE) ==========

    internal Guid OpenChildWindowInSession<TViewModel>(Guid sessionId, Guid parentWindowId)
        where TViewModel : IViewModel
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            throw new InvalidOperationException($"Session {sessionId} not found");

        CleanupDeadWindows();

        if (!_windowsByGuid.TryGetValue(parentWindowId, out var parentInfo) || !parentInfo.IsAlive)
        {
            var message = $"Parent window {parentWindowId} not found or no longer alive";
            _logger.LogError("[WINDOW_SERVICE] {Message}", message);
            throw new InvalidOperationException(message);
        }

        var windowId = Guid.NewGuid();
        session.WindowIds.Add(windowId);
        _windowToSession[windowId] = sessionId;

        return OpenWindowInternal<TViewModel>(
            windowId: windowId,
            parentScope: session.Scope,
            sessionId: sessionId,
            parentWindowId: parentWindowId,
            options: null);
    }

    internal Guid OpenChildWindowInSession<TViewModel, TOptions>(Guid sessionId, Guid parentWindowId, TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            throw new InvalidOperationException($"Session {sessionId} not found");

        CleanupDeadWindows();

        if (!_windowsByGuid.TryGetValue(parentWindowId, out var parentInfo) || !parentInfo.IsAlive)
        {
            var message = $"Parent window {parentWindowId} not found or no longer alive";
            _logger.LogError("[WINDOW_SERVICE] {Message}", message);
            throw new InvalidOperationException(message);
        }

        var windowId = Guid.NewGuid();
        session.WindowIds.Add(windowId);
        _windowToSession[windowId] = sessionId;

        return OpenWindowInternal<TViewModel>(
            windowId: windowId,
            parentScope: session.Scope,
            sessionId: sessionId,
            parentWindowId: parentWindowId,
            options: options);
    }

    // ========== CLOSE WINDOW ==========

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
            window.Close();
        }
    }

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

    public void CloseDialog<TViewModel, TResult>(Guid windowId, TResult result)
        where TViewModel : IDialogViewModel
        where TResult : IVmResult
    {
        // For now, just close the window
        // TODO: Handle dialog result properly
        Close(windowId);
    }

    public void CloseDialog<TViewModel>(Guid windowId) where TViewModel : IDialogViewModel
    {
        Close(windowId);
    }

    // ========== INTERNAL IMPLEMENTATION ==========

    private Guid OpenWindowInternal<TViewModel>(
        Guid windowId,
        ILifetimeScope parentScope,
        Guid? sessionId,
        Guid? parentWindowId,
        object? options) where TViewModel : IViewModel
    {
        var viewModelType = typeof(TViewModel);
        
        _logger.LogInformation("[WINDOW_SERVICE] Opening window {ViewModelType} (WindowId: {WindowId}, Session: {SessionId}, Parent: {ParentId})",
            viewModelType.Name, windowId, sessionId?.ToString() ?? "none", parentWindowId?.ToString() ?? "none");

        // 1. Create ViewModel from parentScope (NOT from window scope!)
        //    ViewModel is created before window, so it uses parent scope
        IViewModel viewModel;
        
        if (options == null)
        {
            // Create from factory using parent scope
            viewModel = parentScope.Resolve<TViewModel>();
        }
        else if (options is IVmParameters vmParams)
        {
            // Create with parameters using TypedParameter
            viewModel = parentScope.Resolve<TViewModel>(
                new TypedParameter(options.GetType(), options));
        }
        else
        {
            throw new InvalidOperationException($"Unsupported options type: {options.GetType()}");
        }

        // 2. Create View (Window)
        //    ViewLocator resolves from parentScope
        //    Window constructor gets ILifetimeScope injected (parentScope)
        //    Window creates its own CHILD scope from that parent
        var view = _viewLocator.ResolveView<TViewModel>();
        
        // 3. Set DataContext
        view.DataContext = viewModel;

        var window = view as Window;
        if (window == null)
        {
            var message = $"View {view.GetType().Name} is not a Window";
            _logger.LogError("[WINDOW_SERVICE] {Message}", message);
            throw new InvalidOperationException(message);
        }

        // 4. Set window owner if this is a child
        if (parentWindowId.HasValue && _windowsByGuid.TryGetValue(parentWindowId.Value, out var parentInfo))
        {
            if (parentInfo.TryGetWindow(out var parentWindow))
            {
                window.Owner = parentWindow;
                
                if (!_childWindows.ContainsKey(parentWindowId.Value))
                    _childWindows[parentWindowId.Value] = new List<Guid>();
                
                _childWindows[parentWindowId.Value].Add(windowId);
                
                _logger.LogDebug("[WINDOW_SERVICE] Set window {WindowId} as child of {ParentId}",
                    windowId, parentWindowId.Value);
            }
        }

        // 5. Track window
        var vmKey = viewModel.GetVmKey();
        var windowInfo = new WindowInfo
        {
            WindowId = windowId,
            VmKey = vmKey,
            WindowRef = new WeakReference<Window>(window),
            ViewModelRef = new WeakReference<IViewModel>(viewModel),
            ParentWindowId = parentWindowId
        };

        _windowsByGuid[windowId] = windowInfo;
        _windowIdsByVmKey[vmKey] = windowId;

        // 6. Subscribe to close event
        window.Closed += (s, e) => OnWindowClosedInternal(windowId, vmKey, viewModel);

        // 7. Show window (InitializeAsync is called by window's code-behind on Loaded event)
        window.Show();

        _logger.LogInformation("[WINDOW_SERVICE] Window {ViewModelType} opened successfully (WindowId: {WindowId}, VmKey: {VmKey})",
            viewModelType.Name, windowId, vmKey);

        return windowId;
    }

    // ========== CLEANUP ==========

    private void OnWindowClosedInternal(Guid windowId, VmKey vmKey, IViewModel viewModel)
    {
        _logger.LogInformation("[WINDOW_SERVICE] Window closed (WindowId: {WindowId}, VmKey: {VmKey})",
            windowId, vmKey);

        CleanupWindow(windowId);

        // Dispose ViewModel if needed (NOTE: Window scope already disposed by ScopedWindow)
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

            // Remove from session if applicable
            if (_windowToSession.TryGetValue(windowId, out var sessionId))
            {
                if (_sessions.TryGetValue(sessionId, out var session))
                {
                    session.WindowIds.Remove(windowId);
                }
                _windowToSession.Remove(windowId);
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

    // ========== HELPER CLASSES ==========

    internal class SessionInfo
    {
        public Guid SessionId { get; init; }
        public string Tag { get; init; } = string.Empty;
        public ILifetimeScope Scope { get; init; } = null!;
        public List<Guid> WindowIds { get; init; } = new();
    }
}
