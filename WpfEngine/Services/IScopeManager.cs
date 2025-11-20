using WpfEngine.Data.Sessions;
using WpfEngine.Services.Sessions;

namespace WpfEngine.Services;

/// <summary>
/// Manages scope sessions - creating, tracking, and closing sessions
/// </summary>
public interface IScopeManager
{
    // ========== SESSION CREATION ==========
    
    /// <summary>
    /// Create a new root session with fluent configuration
    /// </summary>
    /// <param name="tag">Scope tag for the session</param>
    /// <returns>Session builder for fluent configuration</returns>
    ISessionBuilder CreateSession(ScopeTag tag);
    
    // ========== SESSION MANAGEMENT ==========
    
    /// <summary>
    /// Get active session by ID
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    IScopeSession? GetSession(Guid sessionId);
    
    /// <summary>
    /// Close specific session and all its children
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    void CloseSession(Guid sessionId);
    
    /// <summary>
    /// Close all active sessions
    /// </summary>
    void CloseAllSessions();
    
    /// <summary>
    /// All currently active sessions
    /// </summary>
    IReadOnlyList<IScopeSession> ActiveSessions { get; }
    
    /// <summary>
    /// Get all child sessions of a parent
    /// </summary>
    /// <param name="parentSessionId">Parent session identifier</param>
    IReadOnlyList<IScopeSession> GetChildSessions(Guid parentSessionId);
    
    /// <summary>
    /// Check if session is active
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    bool IsSessionActive(Guid sessionId);
    
    // ========== EVENTS ==========
    
    /// <summary>
    /// Raised when a session is created
    /// </summary>
    event EventHandler<SessionEventArgs>? SessionCreated;
    
    /// <summary>
    /// Raised when a session is closed
    /// </summary>
    event EventHandler<SessionEventArgs>? SessionClosed;
}

