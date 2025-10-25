using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.Scopes;
using WpfEngine.Core.ViewModels;
using WpfEngine.Core.Views;
using WpfEngine.Services.WindowTracking;

namespace WpfEngine.Core.Services.Autofac;

/// <summary>
/// Window service with hierarchical scope support
/// 
/// KEY PRINCIPLES:
/// 1. ViewModel is resolved from PARENT scope (sees shared services)
/// 2. Window is resolved from PARENT scope and creates its own CHILD scope
/// 3. If ViewModel is Shell, its content is resolved from WINDOW scope
/// 4. Sessions can share services across multiple windows via InstancePerMatchingLifetimeScope
/// </summary>
public class WindowServiceRefactored : IWindowService
{
    private readonly ILifetimeScope _rootScope;
    private readonly IViewLocatorService _viewLocator;
    private readonly ILogger<WindowServiceRefactored> _logger;

    // Track windows
    private readonly Dictionary<Guid, WindowInfo> _windowsByGuid = new();
    private readonly Dictionary<VmKey, Guid> _windowIdsByVmKey = new();
    
    // Track parent-child relationships
    private readonly Dictionary<Guid, List<Guid>> _childWindows = new();
    
    // Track sessions
    private readonly Dictionary<Guid, SessionInfo> _sessions = new();

    public WindowServiceRefactored(
        ILifetimeScope rootScope,
        IViewLocatorService viewLocator,
        ILogger<WindowServiceRefactored> logger)
    {
        _rootScope = rootScope;
        _viewLocator = viewLocator;
        _logger = logger;
    }

    public event EventHandler<WindowClosedEventArgs>? WindowClosed;

    // ========== SESSION MANAGEMENT ==========

    /// <summary>
    /// Creates a new session scope for sharing services across windows
    /// Use InstancePerMatchingLifetimeScope with matching tag pattern
    /// </summary>
    public Guid CreateSession(string sessionName)
    {
        var sessionId = Guid.NewGuid();
        var scopeTag = ScopeTag.WorkflowSession(sessionName, sessionId);
        
        var sessionScope = _rootScope.BeginLifetimeScope(scopeTag.ToString());
        
        _sessions[sessionId] = new SessionInfo
        {
            SessionId = sessionId,
            ScopeTag = scopeTag,
            Scope = sessionScope,
            WindowIds = new List<Guid>()
        };
        
        _logger.LogInformation("[SESSION] Created session '{SessionName}' (ID: {SessionId})", 
            sessionName, sessionId);
        
        return sessionId;
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
        
        // Close all windows in session (from copy to avoid modification during iteration)
        foreach (var windowId in session.WindowIds.ToList())
        {
            Close(windowId);
        }
        
        // Dispose session scope
        session.Scope.Dispose();
        _sessions.Remove(sessionId);
        
        _logger.LogInformation("[SESSION] Session {SessionId} disposed", sessionId);
    }

    // ========== OPEN WINDOW ==========

    public Guid OpenWindow<TViewModel>() where TViewModel : IViewModel
    {
        var windowId = Guid.NewGuid();
        var scopeTag = ScopeTag.Window(typeof(TViewModel).Name, windowId);
        
        return OpenWindowInternal<TViewModel>(
            windowId: windowId,
            scopeTag: scopeTag,
            parentScope: _rootScope,
            sessionId: null,
            parentWindowId: null,
            options: null);
    }

    public Guid OpenWindow<TViewModel, TOptions>(TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters
    {
        var windowId = options.CorrelationId; // Use CorrelationId as windowId
        var scopeTag = ScopeTag.Window(typeof(TViewModel).Name, windowId);
        
        return OpenWindowInternal<TViewModel, TOptions>(
            windowId: windowId,
            scopeTag: scopeTag,
            parentScope: _rootScope,
            sessionId: null,
            parentWindowId: null,
            options: options);
    }

    // ========== OPEN WINDOW IN SESSION ==========

    public Guid OpenWindowInSession<TViewModel>(Guid sessionId) where TViewModel : IViewModel
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            throw new InvalidOperationException($"Session {sessionId} not found");

        var windowId = Guid.NewGuid();
        var scopeTag = ScopeTag.Window(typeof(TViewModel).Name, windowId);

        session.WindowIds.Add(windowId);

        return OpenWindowInternal<TViewModel>(
            windowId: windowId,
            scopeTag: scopeTag,
            parentScope: session.Scope,  // ← Parent is SESSION scope!
            sessionId: sessionId,
            parentWindowId: null,
            options: null);
    }

    public Guid OpenWindowInSession<TViewModel, TOptions>(Guid sessionId, TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            throw new InvalidOperationException($"Session {sessionId} not found");

        var windowId = options.CorrelationId;
        var scopeTag = ScopeTag.Window(typeof(TViewModel).Name, windowId);

        session.WindowIds.Add(windowId);

        return OpenWindowInternal<TViewModel, TOptions>(
            windowId: windowId,
            scopeTag: scopeTag,
            parentScope: session.Scope,  // ← Parent is SESSION scope!
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
            throw new InvalidOperationException($"Parent window {parentWindowId} not found or no longer alive");

        var windowId = Guid.NewGuid();
        var scopeTag = ScopeTag.Window(typeof(TViewModel).Name, windowId);

        // Child uses SAME parent scope as parent window (shares session services!)
        var parentScope = GetWindowParentScope(parentWindowId);

        return OpenWindowInternal<TViewModel>(
            windowId: windowId,
            scopeTag: scopeTag,
            parentScope: parentScope,
            sessionId: parentInfo.SessionId,
            parentWindowId: parentWindowId,
            options: null);
    }

    public Guid OpenChildWindow<TViewModel, TOptions>(Guid parentWindowId, TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters
    {
        CleanupDeadWindows();
        
        if (!_windowsByGuid.TryGetValue(parentWindowId, out var parentInfo) || !parentInfo.IsAlive)
            throw new InvalidOperationException($"Parent window {parentWindowId} not found or no longer alive");

        var windowId = options.CorrelationId;
        var scopeTag = ScopeTag.Window(typeof(TViewModel).Name, windowId);

        // Child uses SAME parent scope as parent window
        var parentScope = GetWindowParentScope(parentWindowId);

        return OpenWindowInternal<TViewModel, TOptions>(
            windowId: windowId,
            scopeTag: scopeTag,
            parentScope: parentScope,
            sessionId: parentInfo.SessionId,
            parentWindowId: parentWindowId,
            options: options);
    }

    // ========== INTERNAL IMPLEMENTATION ==========

    private Guid OpenWindowInternal<TViewModel>(
        Guid windowId,
        ScopeTag scopeTag,
        ILifetimeScope parentScope,
        Guid? sessionId,
        Guid? parentWindowId,
        object? options) where TViewModel : IViewModel
    {
        _logger.LogInformation(
            "[WINDOW_SERVICE] Opening {ViewModelType} (WindowId: {WindowId}, Session: {SessionId}, Parent: {ParentId})",
            typeof(TViewModel).Name, windowId, sessionId?.ToString() ?? "none", parentWindowId?.ToString() ?? "none");

        // 1. Resolve ViewModel from PARENT scope (sees shared services!)
        IViewModel viewModel;
        if (options is IVmParameters vmParams)
        {
            viewModel = parentScope.Resolve<TViewModel>(new TypedParameter(vmParams.GetType(), vmParams));
        }
        else
        {
            viewModel = parentScope.Resolve<TViewModel>();
        }

        // 2. Resolve View (Window) from PARENT scope
        //    Window constructor receives parentScope and creates child scope
        var view = _viewLocator.ResolveView(typeof(TViewModel));
        
        if (view is not Window window)
            throw new InvalidOperationException($"View for {typeof(TViewModel).Name} is not a Window");

        // 3. Set ViewModel as DataContext
        view.DataContext = viewModel;

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
                    windowId, parentWindowId);
            }
        }

        // 5. Track window
        var vmKey = viewModel.GetVmKey();
        var windowInfo = new WindowInfo
        {
            WindowId = windowId,
            VmKey = vmKey,
            ScopeTag = scopeTag,
            WindowRef = new WeakReference<Window>(window),
            ViewModelRef = new WeakReference<IViewModel>(viewModel),
            ParentWindowId = parentWindowId,
            SessionId = sessionId
        };

        _windowsByGuid[windowId] = windowInfo;
        _windowIdsByVmKey[vmKey] = windowId;

        // 6. Subscribe to close event
        window.Closed += (s, e) => OnWindowClosedInternal(windowId, vmKey, viewModel);

        // 7. Initialize ViewModel if needed
        if (viewModel is IInitializable initializable)
        {
            _logger.LogDebug("[WINDOW_SERVICE] Initializing {ViewModelType}", typeof(TViewModel).Name);
            _ = initializable.InitializeAsync(); // Fire and forget
        }

        // 8. Show window
        window.Show();

        _logger.LogInformation("[WINDOW_SERVICE] Window {ViewModelType} opened (WindowId: {WindowId})",
            typeof(TViewModel).Name, windowId);

        return windowId;
    }

    private Guid OpenWindowInternal<TViewModel, TOptions>(
        Guid windowId,
        ScopeTag scopeTag,
        ILifetimeScope parentScope,
        Guid? sessionId,
        Guid? parentWindowId,
        TOptions? options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters
    {
        return OpenWindowInternal<TViewModel>(windowId, scopeTag, parentScope, sessionId, parentWindowId, options);
    }

    // ========== CLOSE OPERATIONS ==========

    public void Close(Guid windowId)
    {
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

        foreach (var childId in children.ToList())
        {
            Close(childId);
        }
    }

    public void CloseDialog<TViewModel, TResult>(Guid windowId, TResult result)
        where TViewModel : IDialogViewModel
        where TResult : IVmResult
    {
        // For dialogs - implementation needed
        Close(windowId);
    }

    public void CloseDialog<TViewModel>(Guid windowId)
        where TViewModel : IDialogViewModel
    {
        Close(windowId);
    }

    public void CloseAllChildWindows(Window parentWindow)
    {
        if (parentWindow == null)
            throw new ArgumentNullException(nameof(parentWindow));

        // Find window ID from window instance
        CleanupDeadWindows();
        var windowInfo = _windowsByGuid.Values
            .FirstOrDefault(info => info.TryGetWindow(out var w) && ReferenceEquals(w, parentWindow));

        if (windowInfo != null)
        {
            CloseAllChildWindows(windowInfo.WindowId);
        }
    }

    // ========== HELPER METHODS ==========

    private ILifetimeScope GetWindowParentScope(Guid windowId)
    {
        if (!_windowsByGuid.TryGetValue(windowId, out var windowInfo))
            throw new InvalidOperationException($"Window {windowId} not found");

        // If window is in a session, return session scope
        if (windowInfo.SessionId.HasValue && _sessions.TryGetValue(windowInfo.SessionId.Value, out var session))
        {
            return session.Scope;
        }

        // Otherwise return root scope
        return _rootScope;
    }

    private void OnWindowClosedInternal(Guid windowId, VmKey vmKey, IViewModel viewModel)
    {
        _logger.LogInformation("[WINDOW_SERVICE] Window {WindowId} closed", windowId);

        CleanupWindow(windowId);

        // Dispose ViewModel
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
            _childWindows.Remove(windowId);

            // Remove from session if applicable
            if (windowInfo.SessionId.HasValue && _sessions.TryGetValue(windowInfo.SessionId.Value, out var session))
            {
                session.WindowIds.Remove(windowId);
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

    // ========== HELPER CLASSES ==========

    private class SessionInfo
    {
        public Guid SessionId { get; init; }
        public ScopeTag ScopeTag { get; init; }
        public ILifetimeScope Scope { get; init; } = null!;
        public List<Guid> WindowIds { get; init; } = new();
    }

    private class WindowInfo
    {
        public Guid WindowId { get; init; }
        public VmKey VmKey { get; init; }
        public ScopeTag ScopeTag { get; init; }
        public WeakReference<Window> WindowRef { get; init; } = null!;
        public WeakReference<IViewModel> ViewModelRef { get; init; } = null!;
        public Guid? ParentWindowId { get; init; }
        public Guid? SessionId { get; init; }

        public bool TryGetWindow(out Window? window) => WindowRef.TryGetTarget(out window);
        public bool TryGetViewModel(out IViewModel? viewModel) => ViewModelRef.TryGetTarget(out viewModel);
        public bool IsAlive => WindowRef.TryGetTarget(out _) && ViewModelRef.TryGetTarget(out _);
    }
}

