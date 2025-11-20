using WpfEngine.Data.Sessions;
using WpfEngine.Services;
using WpfEngine.Services.Sessions;

namespace WpfEngine.Extensions;

/// <summary>
/// Extension methods for IScopeManager to provide convenient factory methods
/// </summary>
public static class ScopeManagerExtensions
{
    // ========== COMMON SESSION TYPES ==========
    
    /// <summary>
    /// Create a database session with auto-save enabled by default
    /// </summary>
    /// <param name="scopeManager">Scope manager</param>
    /// <param name="contextName">Optional database context name</param>
    /// <returns>Session builder for fluent configuration</returns>
    public static ISessionBuilder CreateDatabaseSession(
        this IScopeManager scopeManager,
        string? contextName = null)
    {
        return scopeManager
            .CreateSession(ScopeTag.Database(contextName))
            .WithAutoSave();
    }
    
    /// <summary>
    /// Create a workflow session with auto-close when empty enabled
    /// </summary>
    /// <param name="scopeManager">Scope manager</param>
    /// <param name="workflowName">Workflow name</param>
    /// <returns>Session builder for fluent configuration</returns>
    public static ISessionBuilder CreateWorkflowSession(
        this IScopeManager scopeManager,
        string workflowName)
    {
        return scopeManager
            .CreateSession(ScopeTag.Workflow(workflowName))
            .AutoCloseWhenEmpty();
    }
    
    /// <summary>
    /// Create a window session
    /// </summary>
    /// <param name="scopeManager">Scope manager</param>
    /// <returns>Session builder for fluent configuration</returns>
    public static ISessionBuilder CreateWindowSession(this IScopeManager scopeManager)
    {
        return scopeManager.CreateSession(ScopeTag.Window());
    }
    
    /// <summary>
    /// Create a request session
    /// </summary>
    /// <param name="scopeManager">Scope manager</param>
    /// <returns>Session builder for fluent configuration</returns>
    public static ISessionBuilder CreateRequestSession(this IScopeManager scopeManager)
    {
        return scopeManager.CreateSession(ScopeTag.Request());
    }
    
    /// <summary>
    /// Create a custom session
    /// </summary>
    /// <param name="scopeManager">Scope manager</param>
    /// <param name="customName">Custom category name</param>
    /// <returns>Session builder for fluent configuration</returns>
    public static ISessionBuilder CreateCustomSession(
        this IScopeManager scopeManager,
        string customName)
    {
        return scopeManager.CreateSession(ScopeTag.Custom(customName));
    }
}

