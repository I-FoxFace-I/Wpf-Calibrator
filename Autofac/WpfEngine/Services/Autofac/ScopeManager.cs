using Autofac;
using WpfEngine.Data.Sessions;
using WpfEngine.Services.Sessions;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using WpfEngine.Services.Sessions.Implementation;

namespace WpfEngine.Services.Autofac;

/// <summary>
/// Manages scope sessions - creating, tracking, and closing sessions
/// </summary>
public class ScopeManager : IScopeManager
{
    private readonly ILifetimeScope _rootScope;
    private readonly ILogger<ScopeManager> _logger;
    
    // Thread-safe session tracking
    private readonly ConcurrentDictionary<Guid, IScopeSession> _sessions = new();
    private readonly ConcurrentDictionary<Guid, Guid> _childToParent = new();
    
    public ScopeManager(ILifetimeScope rootScope, ILogger<ScopeManager> logger)
    {
        _rootScope = rootScope ?? throw new ArgumentNullException(nameof(rootScope));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _logger.LogInformation("[SCOPE_MANAGER] Service initialized");
    }
    
    // ========== SESSION CREATION ==========
    
    public ISessionBuilder CreateSession(ScopeTag tag)
    {
        _logger.LogDebug("[SCOPE_MANAGER] Creating session builder with tag {Tag}", tag);
        
        return new SessionBuilder(
            _rootScope,
            tag,
            this,
            null,
            _logger);
    }
    
    // ========== SESSION MANAGEMENT ==========
    
    public IScopeSession? GetSession(Guid sessionId)
    {
        return _sessions.TryGetValue(sessionId, out var session) ? session : null;
    }
    
    public void CloseSession(Guid sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            _logger.LogWarning("[SCOPE_MANAGER] Session {SessionId} not found", sessionId);
            return;
        }
        
        _logger.LogInformation("[SCOPE_MANAGER] Closing session {SessionId}", sessionId);
        
        // Remove from tracking first to prevent re-entry
        _sessions.TryRemove(sessionId, out _);
        
        // Close child sessions first
        var children = GetChildSessions(sessionId);
        foreach (var child in children)
        {
            CloseSession(child.SessionId);
        }
        
        // Close the session (this will raise Closed event, but OnSessionClosed won't call CloseSession again
        // because session is no longer tracked)
        session.Close();
        
        // Remove parent-child relationship
        _childToParent.TryRemove(sessionId, out _);
        
        // Raise event
        SessionClosed?.Invoke(this, new SessionEventArgs(session.SessionId, session.Tag));
        
        // Dispose
        session.Dispose();
        
        _logger.LogInformation("[SCOPE_MANAGER] Session {SessionId} closed", sessionId);
    }
    
    public void CloseAllSessions()
    {
        _logger.LogInformation("[SCOPE_MANAGER] Closing all sessions");
        
        // Get root sessions (no parent)
        var rootSessions = _sessions.Values
            .Where(s => !_childToParent.ContainsKey(s.SessionId))
            .ToList();
        
        foreach (var session in rootSessions)
        {
            CloseSession(session.SessionId);
        }
        
        _logger.LogInformation("[SCOPE_MANAGER] All sessions closed");
    }
    
    public IReadOnlyList<IScopeSession> ActiveSessions
    {
        get => _sessions.Values.Where(s => s.IsActive).ToList().AsReadOnly();
    }
    
    public IReadOnlyList<IScopeSession> GetChildSessions(Guid parentSessionId)
    {
        return _childToParent
            .Where(kvp => kvp.Value == parentSessionId)
            .Select(kvp => _sessions.TryGetValue(kvp.Key, out var session) ? session : null)
            .Where(s => s != null)
            .Cast<IScopeSession>()
            .ToList()
            .AsReadOnly();
    }
    
    public bool IsSessionActive(Guid sessionId)
    {
        return _sessions.TryGetValue(sessionId, out var session) && session.IsActive;
    }
    
    // ========== INTERNAL METHODS ==========
    
    internal void TrackSession(IScopeSession session)
    {
        if (_sessions.TryAdd(session.SessionId, session))
        {
            _logger.LogDebug("[SCOPE_MANAGER] Tracked session {SessionId}", session.SessionId);
            
            // Track parent-child relationship
            if (session.Parent != null)
            {
                _childToParent.TryAdd(session.SessionId, session.Parent.SessionId);
                
                if (session.Parent is ScopeSession parentSession)
                {
                    parentSession.TrackChild(session);
                }
            }
            
            // Raise event
            SessionCreated?.Invoke(this, new SessionEventArgs(session.SessionId, session.Tag));
        }
        else
        {
            _logger.LogWarning("[SCOPE_MANAGER] Failed to track session {SessionId}", session.SessionId);
        }
    }
    
    internal void UntrackSession(Guid sessionId)
    {
        if (_sessions.TryRemove(sessionId, out _))
        {
            _logger.LogDebug("[SCOPE_MANAGER] Untracked session {SessionId}", sessionId);
            _childToParent.TryRemove(sessionId, out _);
        }
    }
    
    // ========== EVENTS ==========
    
    public event EventHandler<SessionEventArgs>? SessionCreated;
    public event EventHandler<SessionEventArgs>? SessionClosed;
}

