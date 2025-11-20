using Autofac;
using Autofac.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WpfEngine.Data.Abstract;
using WpfEngine.Data.Sessions;
using WpfEngine.Data.Windows.Events;
using WpfEngine.Services.Sessions;
using WpfEngine.Services.Sessions.Implementation;
using WpfEngine.ViewModels;

namespace WpfEngine.Services.Autofac;

/// <summary>
/// Implementation of scope session
/// </summary>
public class ScopeSession : IScopeSession
{
    private readonly ILifetimeScope _scope;
    private readonly ScopeTag _tag;
    private readonly IScopeManager? _scopeManager;
    private readonly IScopeSession? _parent;
    private readonly bool _autoSave;
    private readonly bool _autoCloseWhenEmpty;
    private readonly Action? _onDispose;
    private readonly ILogger? _logger;
    
    private readonly ConcurrentBag<IScopeSession> _children = new();
    private readonly ISessionWindowHost? _windowContext;
    private int _disposed; // 0 = false, 1 = true (thread-safe)
    private int _disposing; // 0 = false, 1 = true (thread-safe)
    private bool _isActive = true;
    
    public ScopeSession(
        ILifetimeScope scope,
        ScopeTag tag,
        IScopeManager? scopeManager = null,
        IScopeSession? parent = null,
        bool autoSave = false,
        bool autoCloseWhenEmpty = false,
        Action? onDispose = null,
        ILogger? logger = null)
    {
        _scope = scope ?? throw new ArgumentNullException(nameof(scope));
        _tag = tag;
        _scopeManager = scopeManager;
        _parent = parent;
        _autoSave = autoSave;
        _autoCloseWhenEmpty = autoCloseWhenEmpty;
        _onDispose = onDispose;
        _logger = logger;
        
        SessionId = Guid.NewGuid();
        
        _logger?.LogInformation("[SCOPE_SESSION] Created session {SessionId} with tag {Tag}", 
            SessionId, _tag);
        
        // Subscribe to Closed event to notify manager for disposal
        Closed += OnSessionClosed;
        
        // Create session window context if window manager is available
        if (_scope.TryResolve<IWindowManager>(out var windowManager))
        {
            _windowContext = new SessionWindowHost(
                SessionId,
                windowManager,
                _scope,
                _scope.ResolveOptional<ILogger<SessionWindowHost>>());
            
            // Subscribe to window events for auto-close functionality
            _windowContext.WindowClosed += OnSessionWindowClosed;
            
            _logger?.LogDebug("[SCOPE_SESSION] Session window context created for session {SessionId}", SessionId);
        }
    }
    
    // ========== SESSION INFO ==========
    
    public Guid SessionId { get; }
    public ScopeTag Tag => _tag;
    public ILifetimeScope Scope => _scope;
    public IScopeSession? Parent => _parent;
    public bool IsActive => _isActive && !IsDisposed;
    public int WindowCount => _windowContext?.WindowCount ?? 0;
    
    // Thread-safe properties
    private bool IsDisposed => Interlocked.CompareExchange(ref _disposed, 0, 0) == 1;
    private bool IsDisposing => Interlocked.CompareExchange(ref _disposing, 0, 0) == 1;

    public Guid? ParentId => _parent?.ParentId;

    // ========== CHILD SESSION CREATION ==========

    public ISessionBuilder CreateChild(ScopeTag tag)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(ScopeSession));
        
        _logger?.LogDebug("[SCOPE_SESSION] Creating child session with tag {Tag}", tag);
        
        return new SessionBuilder(
            _scope,
            tag,
            _scopeManager,
            this,
            _logger);
    }
    
    // ========== SERVICE RESOLUTION ==========
    
    public TService Resolve<TService>() where TService : notnull
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(ScopeSession));
        
        return _scope.Resolve<TService>();
    }
    
    public bool TryResolve<TService>(out TService? service) where TService : class
    {
        if (IsDisposed)
        {
            service = default;
            return false;
        }
        
        return _scope.TryResolve(out service);
    }
    
    // ========== DATABASE OPERATIONS ==========
    
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(ScopeSession));
        
        if (!_tag.IsDatabase())
        {
            _logger?.LogWarning("[SCOPE_SESSION] SaveChangesAsync called on non-database session {SessionId}", 
                SessionId);
            return;
        }
        
        _logger?.LogDebug("[SCOPE_SESSION] Saving changes for session {SessionId}", SessionId);
        
        // Try to find DbContext in scope
        var dbContexts = _scope.ComponentRegistry
            .Registrations
            .SelectMany(r => r.Services)
            .OfType<TypedService>()
            .Where(s => typeof(DbContext).IsAssignableFrom(s.ServiceType))
            .Select(s => s.ServiceType)
            .Distinct()
            .ToList();
        
        foreach (var contextType in dbContexts)
        {
            if (_scope.TryResolve(contextType, out var contextObj) && contextObj is DbContext dbContext)
            {
                await dbContext.SaveChangesAsync(ct);
                _logger?.LogDebug("[SCOPE_SESSION] Saved changes for {ContextType}", contextType.Name);
            }
        }
    }
    
    public void Rollback()
    {
        if (IsDisposed)
            return;
        
        if (!_tag.IsDatabase())
            return;
        
        _logger?.LogDebug("[SCOPE_SESSION] Rolling back changes for session {SessionId}", SessionId);
        
        // Find and rollback all DbContexts
        var dbContexts = _scope.ComponentRegistry
            .Registrations
            .SelectMany(r => r.Services)
            .OfType<TypedService>()
            .Where(s => typeof(DbContext).IsAssignableFrom(s.ServiceType))
            .Select(s => s.ServiceType)
            .Distinct()
            .ToList();
        
        foreach (var contextType in dbContexts)
        {
            if (_scope.TryResolve(contextType, out var contextObj) && contextObj is DbContext dbContext)
            {
                // Clear change tracker
                dbContext.ChangeTracker.Clear();
                _logger?.LogDebug("[SCOPE_SESSION] Rolled back {ContextType}", contextType.Name);
            }
        }
    }
    
    // ========== INTERNAL HELPERS ==========
    
    private void OnSessionClosed(object? sender, EventArgs e)
    {
        // Notify manager to dispose this session
        // Only if Close() was called directly (not from CloseSession which already handles disposal)
        if (_scopeManager is ScopeManager scopeMgr && !IsDisposed && !IsDisposing)
        {
            _logger?.LogDebug("[SCOPE_SESSION] Session {SessionId} closed, notifying manager for disposal", SessionId);
            // Use Task.Run to avoid blocking if manager does synchronous work
            Task.Run(() =>
            {
                try
                {
                    // Check if session is still tracked (not already being closed by manager)
                    if (scopeMgr.IsSessionActive(SessionId))
                    {
                        scopeMgr.CloseSession(SessionId);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "[SCOPE_SESSION] Error notifying manager to dispose session {SessionId}", SessionId);
                }
            });
        }
    }
    
    private void OnSessionWindowClosed(object? sender, WindowClosedEventArgs e)
    {
        _logger?.LogDebug("[SCOPE_SESSION] Window {WindowId} closed, remaining: {Count}", 
            e.WindowId, WindowCount);
        
        if (_autoCloseWhenEmpty && WindowCount == 0)
        {
            _logger?.LogInformation("[SCOPE_SESSION] Auto-closing session {SessionId} (no windows)", 
                SessionId);
            Close();
        }
    }
    
    internal void SaveIfAutoSave()
    {
        if (_autoSave && _tag.IsDatabase())
        {
            SaveChangesAsync().GetAwaiter().GetResult();
        }
    }
    
    internal async Task SaveIfAutoSaveAsync()
    {
        if (_autoSave && _tag.IsDatabase())
        {
            await SaveChangesAsync();
        }
    }
    
    internal void TrackChild(IScopeSession child)
    {
        _children.Add(child);
    }
    
    // ========== WINDOW MANAGEMENT ==========
    
    public Guid OpenWindow<TViewModel>() where TViewModel : IViewModel
    {
        if (IsDisposed) throw new ObjectDisposedException(nameof(ScopeSession));
        
        if (_windowContext == null)
            throw new InvalidOperationException("Window context not available for this session");
        
        _logger?.LogDebug("[SCOPE_SESSION] Opening window {ViewModel} in session {SessionId}",
            typeof(TViewModel).Name, SessionId);
        
        var windowId = _windowContext.OpenWindow<TViewModel>();
        
        _logger?.LogInformation("[SCOPE_SESSION] Opened window {ViewModel} (ID: {WindowId}) in session {SessionId}",
            typeof(TViewModel).Name, windowId, SessionId);
        
        return windowId;
    }
    
    public Guid OpenWindow<TViewModel, TParameters>(TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(ScopeSession));
        
        if (_windowContext == null)
            throw new InvalidOperationException("Window context not available for this session");
        
        _logger?.LogDebug("[SCOPE_SESSION] Opening window {ViewModel} with parameters in session {SessionId}",
            typeof(TViewModel).Name, SessionId);
        
        var windowId = _windowContext.OpenWindow<TViewModel, TParameters>(parameters);
        
        _logger?.LogInformation("[SCOPE_SESSION] Opened window {ViewModel} (ID: {WindowId}) with parameters in session {SessionId}",
            typeof(TViewModel).Name, windowId, SessionId);
        
        return windowId;
    }
    
    // ========== LIFECYCLE ==========
    
    public void Close()
    {
        if (!_isActive || IsDisposed)
            return;
        
        _logger?.LogInformation("[SCOPE_SESSION] Closing session {SessionId}", SessionId);
        
        _isActive = false;
        
        // Close all children first
        foreach (var child in _children)
        {
            try
            {
                child.Close();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[SCOPE_SESSION] Error closing child session");
            }
        }
        
        // Close all windows in this session using window context
        try
        {
            _windowContext?.CloseAllWindows();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[SCOPE_SESSION] Error closing windows in session {SessionId}", SessionId);
        }
        
        Closed?.Invoke(this, EventArgs.Empty);
        
        // Manager will handle disposal via Closed event handler
        // If manager is not available, dispose directly
        if (_scopeManager == null && !IsDisposing && !IsDisposed)
        {
            _logger?.LogDebug("[SCOPE_SESSION] No manager available, disposing directly");
            Dispose();
        }
    }
    
    public async Task CloseAsync()
    {
        if (!_isActive || IsDisposed)
            return;
        
        _logger?.LogInformation("[SCOPE_SESSION] Closing session {SessionId} asynchronously", SessionId);
        
        _isActive = false;
        
        // Close all children first
        foreach (var child in _children)
        {
            try
            {
                await child.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[SCOPE_SESSION] Error closing child session");
            }
        }
        
        // Close all windows in this session using window context
        try
        {
            _windowContext?.CloseAllWindows();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[SCOPE_SESSION] Error closing windows in session {SessionId}", SessionId);
        }
        
        Closed?.Invoke(this, EventArgs.Empty);
        
        // Manager will handle disposal via Closed event handler
        // If manager is not available, dispose directly
        if (_scopeManager == null && !IsDisposing && !IsDisposed)
        {
            _logger?.LogDebug("[SCOPE_SESSION] No manager available, disposing directly");
            await DisposeAsync();
        }
    }
    
    public event EventHandler? Closed;
    
    // ========== DISPOSE ==========
    
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        // Try to set _disposing flag atomically
        if (Interlocked.CompareExchange(ref _disposing, 1, 0) != 0)
        {
            // Already disposing or disposed
            return;
        }
        
        try
        {
            if (disposing)
            {
                _logger?.LogDebug("[SCOPE_SESSION] Disposing session {SessionId}", SessionId);
                
                Close();
                
                _onDispose?.Invoke();
                
                // Untrack from scope manager
                if (_scopeManager is ScopeManager scopeMgr)
                {
                    scopeMgr.UntrackSession(SessionId);
                }
                
                // Dispose window context
                try
                {
                    _windowContext?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "[SCOPE_SESSION] Error disposing window context");
                }
                
                // Dispose children
                foreach (var child in _children)
                {
                    try
                    {
                        child.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "[SCOPE_SESSION] Error disposing child session");
                    }
                }
                
                _children.Clear();
                
                // Dispose scope
                try
                {
                    _scope?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "[SCOPE_SESSION] Error disposing scope");
                }
            }
        }
        finally
        {
            // Set disposed flag atomically
            Interlocked.Exchange(ref _disposed, 1);
            Interlocked.Exchange(ref _disposing, 0);
            
            _logger?.LogInformation("[SCOPE_SESSION] Disposed session {SessionId}", SessionId);
        }
    }
    
    protected virtual async ValueTask DisposeAsyncCore()
    {
        // Try to set _disposing flag atomically
        if (Interlocked.CompareExchange(ref _disposing, 1, 0) != 0)
        {
            // Already disposing or disposed
            return;
        }
        
        try
        {
            _logger?.LogDebug("[SCOPE_SESSION] Disposing session {SessionId} asynchronously", SessionId);
            
            await CloseAsync();
            
            _onDispose?.Invoke();
            
            // Untrack from scope manager
            if (_scopeManager is ScopeManager scopeMgr)
            {
                scopeMgr.UntrackSession(SessionId);
            }
            
            // Dispose window context
            try
            {
                _windowContext?.Dispose();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[SCOPE_SESSION] Error disposing window context");
            }
            
            // Dispose children
            foreach (var child in _children)
            {
                try
                {
                    await child.DisposeAsync();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "[SCOPE_SESSION] Error disposing child session");
                }
            }
            
            _children.Clear();
            
            // Dispose scope
            try
            {
                if (_scope is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync();
                else
                    _scope?.Dispose();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[SCOPE_SESSION] Error disposing scope");
            }
        }
        finally
        {
            // Set disposed flag atomically
            Interlocked.Exchange(ref _disposed, 1);
            Interlocked.Exchange(ref _disposing, 0);
            
            _logger?.LogInformation("[SCOPE_SESSION] Disposed session {SessionId}", SessionId);
        }
    }
}

