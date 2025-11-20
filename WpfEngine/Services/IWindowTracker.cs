using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Autofac;
using WpfEngine.Services.Metadata;

namespace WpfEngine.Services;

// ========== WINDOW TRACKER ==========

/// <summary>
/// Central storage/cache for window metadata and relationships
/// Provides data but doesn't manage lifecycle
/// </summary>
public interface IWindowTracker
{
    // ========== TRACKING ==========

    /// <summary>
    /// Track a window with its metadata
    /// </summary>
    void Track(Guid windowId, WindowMetadata metadata);

    /// <summary>
    /// Stop tracking a window and clear its data
    /// </summary>
    void Untrack(Guid windowId);

    /// <summary>
    /// Update window metadata
    /// </summary>
    void WithMetadata(Guid windowId, Action<WindowMetadata> update);

    /// <summary>
    /// Update window metadata
    /// </summary>
    TResult? WithMetadata<TResult>(Guid windowId, Func<WindowMetadata, TResult> func, TResult? defaultValue = default);

    // ========== TRACKING STATUS ==========
    /// <summary>
    /// Check if a window is open
    /// </summary>
    bool IsWindowOpen(Guid windowId);

    /// <summary>
    /// Get all tracked window IDs
    /// </summary>
    IReadOnlyList<Guid> OpenWindows { get; }

    /// <summary>
    /// Get all child window IDs for a parent
    /// </summary>
    IReadOnlyList<Guid> GetChildWindows(Guid parentWindowId);

    /// <summary>
    /// Get the ViewModel type for a window
    /// </summary>
    Type? GetWindowViewModelType(Guid windowId);

    
    // ========== RETRIEVAL ==========

    /// <summary>
    /// Get window metadata
    /// </summary>
    WindowMetadata? GetMetadata(Guid windowId);

    /// <summary>
    /// Try get window metadata if still window is still alive
    /// </summary>
    bool TryGetMetadata(Guid windowId,[NotNullWhen(true)] out WindowMetadata? metadata);

    /// <summary>
    /// Find windows matching predicate
    /// </summary>
    IReadOnlyList<WindowMetadata> Find(Predicate<WindowMetadata> predicate);

    // ========== SCOPE MANAGEMENT ==========

    /// <summary>
    /// Get window's own scope (if it has one)
    /// </summary>
    ILifetimeScope? GetWindowScope(Guid windowId);

    // ========== RELATIONSHIPS ==========

    /// <summary>
    /// Set parent-child relationship
    /// </summary>
    void SetParent(Guid childId, Guid parentId);

    /// <summary>
    /// Get parent window ID if window is a child
    /// </summary>
    Guid? GetParent(Guid windowId);

    /// <summary>
    /// Get all descendants (recursive)
    /// </summary>
    IReadOnlyList<Guid> GetDescendants(Guid parentId);

    // ========== SESSION ASSOCIATION ==========

    /// <summary>
    /// Associate window with session
    /// </summary>
    void AssociateWithSession(Guid windowId, Guid sessionId);

    /// <summary>
    /// Get window's session ID
    /// </summary>
    Guid? GetSessionId(Guid windowId);

    /// <summary>
    /// Get all windows in session
    /// </summary>
    IReadOnlyList<Guid> GetSessionWindows(Guid sessionId);
}