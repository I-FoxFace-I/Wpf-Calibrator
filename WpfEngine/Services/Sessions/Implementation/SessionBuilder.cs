using Autofac;
using Microsoft.Extensions.Logging;
using WpfEngine.Data.Abstract;
using WpfEngine.Data.Sessions;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;
using WpfEngine.Services.Sessions;
using WpfEngine.ViewModels;

namespace WpfEngine.Services.Sessions.Implementation;

/// <summary>
/// Base session builder implementation
/// </summary>
public class SessionBuilder : ISessionBuilder
{
    protected readonly ILifetimeScope _parentScope;
    protected readonly ScopeTag _tag;
    protected readonly IScopeManager? _scopeManager;
    protected readonly IScopeSession? _parentSession;
    protected readonly List<Action<ContainerBuilder>> _configurations = new();
    protected readonly ILogger? _logger;
    
    protected bool _autoSave;
    protected bool _autoCloseWhenEmpty;
    protected Action? _onDispose;
    protected bool _disposed;
    
    public SessionBuilder(
        ILifetimeScope parentScope,
        ScopeTag tag,
        IScopeManager? scopeManager = null,
        IScopeSession? parentSession = null,
        ILogger? logger = null)
    {
        _parentScope = parentScope ?? throw new ArgumentNullException(nameof(parentScope));
        _tag = tag;
        _scopeManager = scopeManager;
        _parentSession = parentSession;
        _logger = logger;
    }
    
    // ========== MODULE REGISTRATION ==========
    
    public virtual ISessionBuilder WithModule<TModule>() where TModule : ISessionModule, new()
    {
        var module = new TModule();
        _configurations.Add(builder => module.ConfigureServices(builder));
        _logger?.LogDebug("[SESSION_BUILDER] Added module {ModuleType}", typeof(TModule).Name);
        return this;
    }
    
    public virtual ISessionBuilder WithModule(Action<ContainerBuilder> configure)
    {
        if (configure == null) throw new ArgumentNullException(nameof(configure));
        _configurations.Add(configure);
        _logger?.LogDebug("[SESSION_BUILDER] Added configuration action");
        return this;
    }
    
    // ========== SERVICE DECLARATION ==========
    
    public virtual ISessionBuilder<T1> WithService<T1>() where T1 : notnull
    {
        _logger?.LogDebug("[SESSION_BUILDER] Declared service {ServiceType}", typeof(T1).Name);
        return new SessionBuilder<T1>(this);
    }
    
    // ========== LIFECYCLE CONFIGURATION ==========
    
    public virtual ISessionBuilder WithAutoSave(bool enabled = true)
    {
        _autoSave = enabled;
        _logger?.LogDebug("[SESSION_BUILDER] Auto-save: {Enabled}", enabled);
        return this;
    }
    
    public virtual ISessionBuilder OnDispose(Action? hook)
    {
        _onDispose = hook;
        _logger?.LogDebug("[SESSION_BUILDER] Added dispose hook");
        return this;
    }
    
    public virtual ISessionBuilder AutoCloseWhenEmpty()
    {
        _autoCloseWhenEmpty = true;
        _logger?.LogDebug("[SESSION_BUILDER] Auto-close when empty enabled");
        return this;
    }
    
    // ========== BUILD ==========
    
    public virtual IScopeSession Build()
    {
        _logger?.LogDebug("[SESSION_BUILDER] Building session with tag {Tag}", _tag);
        
        // Create lifetime scope
        var scope = _configurations.Any()
            ? _parentScope.BeginLifetimeScope(_tag.ToAutofacTag(), b =>
            {
                foreach (var config in _configurations)
                {
                    config(b);
                }
            })
            : _parentScope.BeginLifetimeScope(_tag.ToAutofacTag());
        
        // Create session
        var session = new ScopeSession(
            scope,
            _tag,
            _scopeManager,
            _parentSession,
            _autoSave,
            _autoCloseWhenEmpty,
            _onDispose,
            scope.ResolveOptional<ILogger<ScopeSession>>());
        
        _logger?.LogInformation("[SESSION_BUILDER] Built session {SessionId} with tag {Tag}", 
            session.SessionId, _tag);
        
        // Track session in manager
        if (_scopeManager is ScopeManager scopeMgr)
        {
            scopeMgr.TrackSession(session);
        }
        
        // Track in parent session
        if (_parentSession is ScopeSession parentScopeSession)
        {
            parentScopeSession.TrackChild(session);
        }
        
        return session;
    }
    
    // ========== WINDOW MANAGEMENT ==========
    
    public virtual IScopeSession OpenWindow<TViewModel>() where TViewModel : IViewModel
    {
        // Build the session first
        var session = Build();
        
        try
        {
            // Use session's OpenWindow method
            session.OpenWindow<TViewModel>();
            
            _logger?.LogInformation(
                "[SESSION_BUILDER] Opened window {ViewModel} in session {SessionId}",
                typeof(TViewModel).Name, session.SessionId);
            
            return session;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, 
                "[SESSION_BUILDER] Failed to open window {ViewModel} in session {SessionId}",
                typeof(TViewModel).Name, session.SessionId);
            
            // Dispose session on failure
            session.Dispose();
            throw;
        }
    }
    
    public virtual IScopeSession OpenWindow<TViewModel, TParameters>(TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters
    {
        // Build the session first
        var session = Build();
        
        try
        {
            // Use session's OpenWindow method
            session.OpenWindow<TViewModel, TParameters>(parameters);
            
            _logger?.LogInformation(
                "[SESSION_BUILDER] Opened window {ViewModel} with parameters in session {SessionId}",
                typeof(TViewModel).Name, session.SessionId);
            
            return session;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, 
                "[SESSION_BUILDER] Failed to open window {ViewModel} in session {SessionId}",
                typeof(TViewModel).Name, session.SessionId);
            
            // Dispose session on failure
            session.Dispose();
            throw;
        }
    }
    
    // ========== DISPOSE ==========
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        _configurations.Clear();
        GC.SuppressFinalize(this);
    }
}

