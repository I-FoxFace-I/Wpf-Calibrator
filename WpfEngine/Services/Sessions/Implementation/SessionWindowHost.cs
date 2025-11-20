using Autofac;
using Microsoft.Extensions.Logging;
using WpfEngine.Data.Abstract;
using WpfEngine.Data.Windows.Events;
using WpfEngine.Services;
using WpfEngine.Services.Sessions;
using WpfEngine.ViewModels;

namespace WpfEngine.Services.Sessions.Implementation;

/// <summary>
/// Window context scoped to a specific session - tracks and manages windows within a session
/// </summary>
internal class SessionWindowHost : ISessionWindowHost
{
    private readonly Guid _sessionId;
    private readonly IWindowManager _windowManager;
    private readonly ILifetimeScope _sessionScope;
    private readonly ILogger? _logger;
    private readonly HashSet<Guid> _sessionWindows = new();
    private readonly object _lock = new();
    private bool _disposed;
    
    public event EventHandler<WindowOpenedEventArgs>? WindowOpened;
    public event EventHandler<WindowClosedEventArgs>? WindowClosed;
    
    public SessionWindowHost(
        Guid sessionId,
        IWindowManager windowManager,
        ILifetimeScope sessionScope,
        ILogger? logger = null)
    {
        _sessionId = sessionId;
        _windowManager = windowManager ?? throw new ArgumentNullException(nameof(windowManager));
        _sessionScope = sessionScope ?? throw new ArgumentNullException(nameof(sessionScope));
        _logger = logger;
        
        // Subscribe to global window events and filter by session
        _windowManager.WindowOpened += OnGlobalWindowOpened;
        _windowManager.WindowClosed += OnGlobalWindowClosed;
        
        _logger?.LogDebug("[SESSION_WINDOW_CTX] Created for session {SessionId}", _sessionId);
    }
    
    public int WindowCount
    {
        get
        {
            lock (_lock)
            {
                return _sessionWindows.Count;
            }
        }
    }
    
    public Guid OpenWindow<TViewModel>() where TViewModel : IViewModel
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SessionWindowHost));
        
        _logger?.LogDebug("[SESSION_WINDOW_CTX] Opening window {ViewModel} in session {SessionId}",
            typeof(TViewModel).Name, _sessionId);
        
        var windowId = _windowManager.OpenWindowInSession<TViewModel>(_sessionId);
        
        lock (_lock)
        {
            _sessionWindows.Add(windowId);
        }
        
        _logger?.LogInformation("[SESSION_WINDOW_CTX] Opened window {WindowId} in session {SessionId}", 
            windowId, _sessionId);
        
        return windowId;
    }
    
    public Guid OpenWindow<TViewModel, TParameters>(TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SessionWindowHost));
        
        _logger?.LogDebug("[SESSION_WINDOW_CTX] Opening window {ViewModel} with parameters in session {SessionId}",
            typeof(TViewModel).Name, _sessionId);
        
        var windowId = _windowManager.OpenWindowInSession<TViewModel, TParameters>(_sessionId, parameters);
        
        lock (_lock)
        {
            _sessionWindows.Add(windowId);
        }
        
        _logger?.LogInformation("[SESSION_WINDOW_CTX] Opened window {WindowId} with parameters in session {SessionId}", 
            windowId, _sessionId);
        
        return windowId;
    }
    
    public async Task<bool?> ShowDialogAsync<TViewModel>() where TViewModel : IViewModel
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SessionWindowHost));
        
        _logger?.LogDebug("[SESSION_WINDOW_CTX] Opening dialog {ViewModel} in session {SessionId}",
            typeof(TViewModel).Name, _sessionId);
        
        // For dialogs, we need to use the scoped window manager
        if (!_sessionScope.TryResolve<IScopedWindowManager>(out var scopedWindowManager))
        {
            throw new InvalidOperationException("IScopedWindowManager not available in session scope");
        }
        
        var dialogId = scopedWindowManager.OpenWindowInSession<TViewModel>(_sessionId);
        
        lock (_lock)
        {
            _sessionWindows.Add(dialogId);
        }
        
        // Wait for dialog to close - we need to track this via events
        var tcs = new TaskCompletionSource<bool?>();
        
        void OnDialogClosed(object? sender, WindowClosedEventArgs e)
        {
            if (e.WindowId == dialogId)
            {
                _windowManager.WindowClosed -= OnDialogClosed;
                tcs.TrySetResult(null); // TODO: Get actual dialog result
            }
        }
        
        _windowManager.WindowClosed += OnDialogClosed;
        
        return await tcs.Task;
    }
    
    public async Task<bool?> ShowDialogAsync<TViewModel, TParameters>(TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SessionWindowHost));
        
        _logger?.LogDebug("[SESSION_WINDOW_CTX] Opening dialog {ViewModel} with parameters in session {SessionId}",
            typeof(TViewModel).Name, _sessionId);
        
        // For dialogs, we need to use the scoped window manager
        if (!_sessionScope.TryResolve<IScopedWindowManager>(out var scopedWindowManager))
        {
            throw new InvalidOperationException("IScopedWindowManager not available in session scope");
        }
        
        var dialogId = scopedWindowManager.OpenWindowInSession<TViewModel, TParameters>(_sessionId, parameters);
        
        lock (_lock)
        {
            _sessionWindows.Add(dialogId);
        }
        
        // Wait for dialog to close
        var tcs = new TaskCompletionSource<bool?>();
        
        void OnDialogClosed(object? sender, WindowClosedEventArgs e)
        {
            if (e.WindowId == dialogId)
            {
                _windowManager.WindowClosed -= OnDialogClosed;
                tcs.TrySetResult(null); // TODO: Get actual dialog result
            }
        }
        
        _windowManager.WindowClosed += OnDialogClosed;
        
        return await tcs.Task;
    }
    
    public void CloseWindow(Guid windowId)
    {
        if (_disposed)
            return;
        
        bool isOurs;
        lock (_lock)
        {
            isOurs = _sessionWindows.Contains(windowId);
        }
        
        if (isOurs)
        {
            _logger?.LogDebug("[SESSION_WINDOW_CTX] Closing window {WindowId}", windowId);
            _windowManager.CloseWindow(windowId);
        }
    }
    
    public void CloseAllWindows()
    {
        if (_disposed)
            return;
        
        List<Guid> windowsToClose;
        lock (_lock)
        {
            windowsToClose = _sessionWindows.ToList();
        }
        
        _logger?.LogInformation("[SESSION_WINDOW_CTX] Closing all {Count} windows in session {SessionId}",
            windowsToClose.Count, _sessionId);
        
        foreach (var windowId in windowsToClose)
        {
            try
            {
                _windowManager.CloseWindow(windowId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[SESSION_WINDOW_CTX] Error closing window {WindowId}", windowId);
            }
        }
    }
    
    private void OnGlobalWindowOpened(object? sender, WindowOpenedEventArgs e)
    {
        // Filter: only our session windows
        if (e.SessionId == _sessionId)
        {
            lock (_lock)
            {
                // Add to tracking if not already there
                if (!_sessionWindows.Contains(e.WindowId))
                {
                    _sessionWindows.Add(e.WindowId);
                }
            }
            
            _logger?.LogDebug("[SESSION_WINDOW_CTX] Window {WindowId} opened in session {SessionId}, total: {Count}",
                e.WindowId, _sessionId, WindowCount);
            
            WindowOpened?.Invoke(this, e);
        }
    }
    
    private void OnGlobalWindowClosed(object? sender, WindowClosedEventArgs e)
    {
        // Filter: only our session windows
        if (e.SessionId == _sessionId)
        {
            bool wasOurs;
            lock (_lock)
            {
                wasOurs = _sessionWindows.Remove(e.WindowId);
            }
            
            if (wasOurs)
            {
                _logger?.LogDebug("[SESSION_WINDOW_CTX] Window {WindowId} closed in session {SessionId}, remaining: {Count}",
                    e.WindowId, _sessionId, WindowCount);
                
                WindowClosed?.Invoke(this, e);
            }
        }
    }
    
    public void Dispose()
    {
        if (_disposed)
            return;
        
        _logger?.LogDebug("[SESSION_WINDOW_CTX] Disposing for session {SessionId}", _sessionId);
        
        // Unsubscribe from events
        _windowManager.WindowOpened -= OnGlobalWindowOpened;
        _windowManager.WindowClosed -= OnGlobalWindowClosed;
        
        // Clear window tracking
        lock (_lock)
        {
            _sessionWindows.Clear();
        }
        
        _disposed = true;
        
        GC.SuppressFinalize(this);
    }
}

