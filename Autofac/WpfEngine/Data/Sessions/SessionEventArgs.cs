namespace WpfEngine.Data.Sessions;

/// <summary>
/// Event arguments for session lifecycle events
/// </summary>
public class SessionEventArgs : EventArgs
{
    /// <summary>
    /// Session identifier
    /// </summary>
    public Guid SessionId { get; }
    
    /// <summary>
    /// Session scope tag
    /// </summary>
    public ScopeTag Tag { get; }
    
    /// <summary>
    /// Timestamp of the event
    /// </summary>
    public DateTime Timestamp { get; }
    
    public SessionEventArgs(Guid sessionId, ScopeTag tag)
    {
        SessionId = sessionId;
        Tag = tag;
        Timestamp = DateTime.UtcNow;
    }
}

