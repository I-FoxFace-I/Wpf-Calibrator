using System;

namespace WpfEngine.Core.Scopes;

/// <summary>
/// Defines scope tag categories for type-safe scope management
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
    /// Workflow session scope - shared across workflow windows
    /// </summary>
    WorkflowSession,
    
    /// <summary>
    /// Dialog scope - for modal dialogs
    /// </summary>
    Dialog,
    
    /// <summary>
    /// Custom scope - user-defined
    /// </summary>
    Custom
}

/// <summary>
/// Strongly-typed scope tag
/// </summary>
public readonly struct ScopeTag : IEquatable<ScopeTag>
{
    public ScopeCategory Category { get; }
    public string Name { get; }
    public Guid Id { get; }

    public ScopeTag(ScopeCategory category, string name, Guid? id = null)
    {
        Category = category;
        Name = name;
        Id = id ?? Guid.NewGuid();
    }

    public static ScopeTag Root() => new(ScopeCategory.Root, "root", Guid.Empty);
    
    public static ScopeTag Window(string windowName, Guid? id = null) 
        => new(ScopeCategory.Window, windowName, id);
    
    public static ScopeTag WorkflowSession(string workflowName, Guid? id = null)
        => new(ScopeCategory.WorkflowSession, workflowName, id);
    
    public static ScopeTag Dialog(string dialogName, Guid? id = null)
        => new(ScopeCategory.Dialog, dialogName, id);
    
    public static ScopeTag Custom(string customName, Guid? id = null)
        => new(ScopeCategory.Custom, customName, id);

    public override string ToString() => $"{Category}:{Name}:{Id}";
    
    public string ToShortString() => $"{Category}:{Name}";

    public bool Equals(ScopeTag other)
    {
        return Category == other.Category && 
               Name == other.Name && 
               Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        return obj is ScopeTag other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Category, Name, Id);
    }

    public static bool operator ==(ScopeTag left, ScopeTag right) => left.Equals(right);
    public static bool operator !=(ScopeTag left, ScopeTag right) => !left.Equals(right);
}

/// <summary>
/// Extensions for working with scope tags
/// </summary>
public static class ScopeTagExtensions
{
    /// <summary>
    /// Checks if scope tag matches category
    /// </summary>
    public static bool IsCategory(this ScopeTag tag, ScopeCategory category)
    {
        return tag.Category == category;
    }

    /// <summary>
    /// Checks if scope tag is workflow session
    /// </summary>
    public static bool IsWorkflowSession(this ScopeTag tag)
    {
        return tag.Category == ScopeCategory.WorkflowSession;
    }

    /// <summary>
    /// Checks if scope tag is window
    /// </summary>
    public static bool IsWindow(this ScopeTag tag)
    {
        return tag.Category == ScopeCategory.Window;
    }
}

