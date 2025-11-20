using WpfEngine.Data.Sessions;
using WpfEngine.Services;
using WpfEngine.Services.Sessions;

namespace WpfEngine.Extensions;

/// <summary>
/// Extension methods for IScopeSession
/// </summary>
public static class ScopeSessionExtensions
{
    // ========== CHILD SESSION CREATION ==========
    
    /// <summary>
    /// Create a child database session with auto-save enabled
    /// </summary>
    public static ISessionBuilder CreateChildDatabaseSession(
        this IScopeSession session,
        string? contextName = null)
    {
        return session
            .CreateChild(ScopeTag.Database(contextName))
            .WithAutoSave();
    }
    
    /// <summary>
    /// Create a child workflow session with auto-close when empty
    /// </summary>
    public static ISessionBuilder CreateChildWorkflowSession(
        this IScopeSession session,
        string workflowName)
    {
        return session
            .CreateChild(ScopeTag.Workflow(workflowName))
            .AutoCloseWhenEmpty();
    }
    
    /// <summary>
    /// Create a child window session
    /// </summary>
    public static ISessionBuilder CreateChildWindowSession(this IScopeSession session)
    {
        return session.CreateChild(ScopeTag.Window());
    }
    
    /// <summary>
    /// Create a child request session
    /// </summary>
    public static ISessionBuilder CreateChildRequestSession(this IScopeSession session)
    {
        return session.CreateChild(ScopeTag.Request());
    }
    
    /// <summary>
    /// Create a child custom session
    /// </summary>
    public static ISessionBuilder CreateChildCustomSession(
        this IScopeSession session,
        string customName)
    {
        return session.CreateChild(ScopeTag.Custom(customName));
    }
}

