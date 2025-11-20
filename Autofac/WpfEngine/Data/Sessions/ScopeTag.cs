namespace WpfEngine.Data.Sessions;

/// <summary>
/// Strongly-typed scope tag for session identification and Autofac scope matching
/// </summary>
public readonly struct ScopeTag : IEquatable<ScopeTag>
{
    /// <summary>
    /// Category of the scope
    /// </summary>
    public ScopeCategory Category { get; }
    
    /// <summary>
    /// Optional name for the scope (e.g., "customer-workflow", "app-context")
    /// For Custom category, this is required and represents the custom category name
    /// </summary>
    public string? Name { get; }
    
    /// <summary>
    /// Creates a new scope tag
    /// </summary>
    /// <param name="category">Scope category</param>
    /// <param name="name">Optional scope name (required for Custom category)</param>
    public ScopeTag(ScopeCategory category, string? name = null)
    {
        Category = category;
        
        // For Custom category, name is required
        if (category == ScopeCategory.Custom && string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required for Custom scope category", nameof(name));
        }
        
        Name = name;
    }
    
    // ========== FACTORY METHODS ==========
    
    /// <summary>
    /// Creates a root application scope tag
    /// </summary>
    public static ScopeTag Root() => new(ScopeCategory.Root);
    
    /// <summary>
    /// Creates a window scope tag
    /// </summary>
    public static ScopeTag Window() => new(ScopeCategory.Window);
    
    /// <summary>
    /// Creates a database scope tag
    /// </summary>
    /// <param name="contextName">Optional database context name</param>
    public static ScopeTag Database(string? contextName = null) 
        => new(ScopeCategory.Database, contextName);

    /// <summary>
    /// Creates a workflow scope tag
    /// </summary>
    /// <param name="workflowName">Workflow name</param>
    public static ScopeTag Workflow(string? workflowName=null)
        => new(ScopeCategory.Workflow, workflowName);
    
    /// <summary>
    /// Creates a request scope tag
    /// </summary>
    public static ScopeTag Request() => new(ScopeCategory.Request);
    
    /// <summary>
    /// Creates a custom scope tag
    /// </summary>
    /// <param name="customName">Custom category name (required)</param>
    public static ScopeTag Custom(string customName) 
        => new(ScopeCategory.Custom, customName ?? throw new ArgumentNullException(nameof(customName)));
    
    // ========== STRING CONVERSION ==========
    
    /// <summary>
    /// Converts to full string representation
    /// </summary>
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(Name))
        {
            return $"{Category}:{Name}";
        }
        
        return Category.ToString();
    }
    
    /// <summary>
    /// Converts to Autofac tag for InstancePerMatchingLifetimeScope
    /// </summary>
    /// <remarks>
    /// For database and workflow scopes, we want to share services across instances
    /// with the same category+name. For window and request scopes, each gets unique scope.
    /// </remarks>
    public string ToAutofacTag()
    {
        // Database and workflow: share by category+name
        if (Category == ScopeCategory.Database || Category == ScopeCategory.Workflow)
        {
            return !string.IsNullOrEmpty(Name) 
                ? $"{Category}:{Name}" 
                : Category.ToString();
        }
        
        // For Custom, use the full representation
        if (Category == ScopeCategory.Custom)
        {
            return ToString();
        }
        
        // Window and request: use category only (each session will have unique scope instance)
        return Category.ToString();
    }
    
    /// <summary>
    /// Short string representation (category:name or just category)
    /// </summary>
    public string ToShortString()
    {
        return ToString();
    }
    
    // ========== EQUALITY ==========
    
    public bool Equals(ScopeTag other)
    {
        return Category == other.Category
            && Name == other.Name;
    }
    
    public override bool Equals(object? obj)
    {
        return obj is ScopeTag other && Equals(other);
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Category, Name);
    }
    
    public static bool operator ==(ScopeTag left, ScopeTag right) => left.Equals(right);
    public static bool operator !=(ScopeTag left, ScopeTag right) => !left.Equals(right);
}

/// <summary>
/// Extension methods for ScopeTag
/// </summary>
public static class ScopeTagExtensions
{
    /// <summary>
    /// Checks if scope tag is a database scope
    /// </summary>
    public static bool IsDatabase(this ScopeTag tag) 
        => tag.Category == ScopeCategory.Database;
    
    /// <summary>
    /// Checks if scope tag is a workflow scope
    /// </summary>
    public static bool IsWorkflow(this ScopeTag tag) 
        => tag.Category == ScopeCategory.Workflow;
    
    /// <summary>
    /// Checks if scope tag is a window scope
    /// </summary>
    public static bool IsWindow(this ScopeTag tag) 
        => tag.Category == ScopeCategory.Window;
    
    /// <summary>
    /// Checks if scope tag is a request scope
    /// </summary>
    public static bool IsRequest(this ScopeTag tag) 
        => tag.Category == ScopeCategory.Request;
    
    /// <summary>
    /// Checks if scope tag is a custom scope
    /// </summary>
    public static bool IsCustom(this ScopeTag tag) 
        => tag.Category == ScopeCategory.Custom;
}

