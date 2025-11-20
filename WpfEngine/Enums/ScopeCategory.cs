namespace WpfEngine.Data.Sessions;

/// <summary>
/// Defines scope categories for type-safe scope management
/// </summary>
public enum ScopeCategory
{
    /// <summary>
    /// Root application scope
    /// </summary>
    Root,
    
    /// <summary>
    /// Window scope - each window has its own
    /// </summary>
    Window,
    
    /// <summary>
    /// Database scope - for database operations and DbContext management
    /// </summary>
    Database,
    
    /// <summary>
    /// Workflow session scope - shared across workflow steps
    /// </summary>
    Workflow,
    
    /// <summary>
    /// Request scope - for request-scoped operations
    /// </summary>
    Request,
    
    /// <summary>
    /// Custom scope - user-defined category
    /// </summary>
    Custom
}

