using System;
using System.Collections.Generic;
using System.Linq;
using WpfEngine.Core.Scopes;
using Microsoft.Extensions.DependencyInjection;

namespace WpfEngine.Extensions;

/// <summary>
/// Helper extensions for working with scope contexts
/// </summary>
public static class ScopeContextExtensions
{
    /// <summary>
    /// Resolves service from scope context
    /// </summary>
    public static TService Resolve<TService>(this IScopeContext scopeContext)
        where TService : notnull
    {
        return scopeContext.ServiceProvider.GetRequiredService<TService>();
    }

    /// <summary>
    /// Tries to resolve service from scope context
    /// </summary>
    public static TService? TryResolve<TService>(this IScopeContext scopeContext)
        where TService : class
    {
        return scopeContext.ServiceProvider.GetService<TService>();
    }

    /// <summary>
    /// Gets all child scopes (non-recursive)
    /// </summary>
    public static IEnumerable<IScopeContext> GetChildren(this IScopeContext scopeContext)
    {
        // This would require tracking children in ScopeContext
        // For now, return empty (implement if needed)
        return Enumerable.Empty<IScopeContext>();
    }

    /// <summary>
    /// Gets scope depth in hierarchy (0 = root)
    /// </summary>
    public static int GetDepth(this IScopeContext scopeContext)
    {
        int depth = 0;
        var current = scopeContext.Parent;
        
        while (current != null)
        {
            depth++;
            current = current.Parent;
        }

        return depth;
    }

    /// <summary>
    /// Gets root scope
    /// </summary>
    public static IScopeContext GetRoot(this IScopeContext scopeContext)
    {
        var current = scopeContext;
        while (current.Parent != null)
        {
            current = current.Parent;
        }
        return current;
    }

    /// <summary>
    /// Gets scope path from root to this scope
    /// </summary>
    public static string GetScopePath(this IScopeContext scopeContext)
    {
        var tags = new List<string>();
        var current = scopeContext;

        while (current != null)
        {
            tags.Insert(0, current.ScopeTag);
            current = current.Parent;
        }

        return string.Join(" → ", tags);
    }

    /// <summary>
    /// Checks if scope is ancestor of another scope
    /// </summary>
    public static bool IsAncestorOf(this IScopeContext ancestor, IScopeContext descendant)
    {
        var current = descendant.Parent;
        
        while (current != null)
        {
            if (current.ScopeId == ancestor.ScopeId)
                return true;
            
            current = current.Parent;
        }

        return false;
    }

    /// <summary>
    /// Checks if scope is descendant of another scope
    /// </summary>
    public static bool IsDescendantOf(this IScopeContext descendant, IScopeContext ancestor)
    {
        return ancestor.IsAncestorOf(descendant);
    }

    /// <summary>
    /// Finds common ancestor of two scopes
    /// </summary>
    public static IScopeContext? FindCommonAncestor(
        this IScopeContext scope1, 
        IScopeContext scope2)
    {
        var ancestors1 = new HashSet<Guid>();
        var current = scope1;

        // Collect all ancestors of scope1
        while (current != null)
        {
            ancestors1.Add(current.ScopeId);
            current = current.Parent;
        }

        // Walk up scope2 until we find common ancestor
        current = scope2;
        while (current != null)
        {
            if (ancestors1.Contains(current.ScopeId))
                return current;
            
            current = current.Parent;
        }

        return null;
    }

    /// <summary>
    /// Creates child scope with builder pattern
    /// </summary>
    public static IScopeContext CreateChildWithSetup(
        this IScopeContext parent,
        string childTag,
        Action<IScopeContext> setup)
    {
        var child = parent.CreateChild(childTag);
        setup(child);
        return child;
    }

    /// <summary>
    /// Registers multiple instances at once
    /// </summary>
    public static IScopeContext RegisterInstances(
        this IScopeContext scopeContext,
        params object[] instances)
    {
        foreach (var instance in instances)
        {
            var method = typeof(IScopeContext)
                .GetMethod(nameof(IScopeContext.RegisterInstance))!
                .MakeGenericMethod(instance.GetType());
            
            method.Invoke(scopeContext, new[] { instance });
        }

        return scopeContext;
    }

    /// <summary>
    /// Tries to resolve from scope hierarchy (walks up)
    /// </summary>
    public static TService? ResolveFromHierarchy<TService>(this IScopeContext scopeContext)
        where TService : class
    {
        // Try current scope first
        var service = scopeContext.TryResolve<TService>();
        if (service != null)
            return service;

        // Try instances
        if (scopeContext.TryResolveInstance<TService>(out var instance))
            return instance;

        return null;
    }
}

/// <summary>
/// Scope diagnostics extensions
/// </summary>
public static class ScopeDiagnosticsExtensions
{
    /// <summary>
    /// Gets diagnostic information about scope
    /// </summary>
    public static ScopeDiagnostics GetDiagnostics(this IScopeContext scopeContext)
    {
        return new ScopeDiagnostics
        {
            ScopeId = scopeContext.ScopeId,
            ScopeTag = scopeContext.ScopeTag,
            Depth = scopeContext.GetDepth(),
            Path = scopeContext.GetScopePath(),
            IsDisposed = scopeContext.IsDisposed,
            ParentScopeId = scopeContext.Parent?.ScopeId
        };
    }

    /// <summary>
    /// Prints scope hierarchy to console (for debugging)
    /// </summary>
    public static void PrintHierarchy(this IScopeContext scopeContext)
    {
        PrintHierarchyRecursive(scopeContext.GetRoot(), 0);
    }

    private static void PrintHierarchyRecursive(IScopeContext scope, int depth)
    {
        var indent = new string(' ', depth * 2);
        var status = scope.IsDisposed ? "[DISPOSED]" : "[ACTIVE]";
        
        Console.WriteLine($"{indent}├─ {scope.ScopeTag} ({scope.ScopeId}) {status}");

        // Would need to track children to print them
        // Implement if needed
    }
}

/// <summary>
/// Scope diagnostics information
/// </summary>
public class ScopeDiagnostics
{
    public Guid ScopeId { get; init; }
    public string ScopeTag { get; init; } = string.Empty;
    public int Depth { get; init; }
    public string Path { get; init; } = string.Empty;
    public bool IsDisposed { get; init; }
    public Guid? ParentScopeId { get; init; }

    public override string ToString()
    {
        return $"Scope: {ScopeTag} (ID: {ScopeId}, Depth: {Depth}, Path: {Path}, Disposed: {IsDisposed})";
    }
}
